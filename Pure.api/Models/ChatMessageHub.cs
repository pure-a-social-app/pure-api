using Pure.Common.Contracts;
using Pure.Common.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Pure.api.Domain.Contracts;
using System.Collections.Generic;

namespace Pure.api.Models
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatMessageHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ChatRoom> _chatRoomRepository;
        private readonly IFileService _fileService;

        private static Dictionary<string, string> _connectionIds = new Dictionary<string, string>();

        public ChatMessageHub(IChatService chatService, 
            IRepository<User> userRepository,
            IRepository<ChatRoom> chatRoomRepository,
            IFileService fileService)
        {
            _chatService = chatService;
            _userRepository = userRepository;
            _chatRoomRepository = chatRoomRepository;
            _fileService = fileService;
        }

        public override async Task OnConnectedAsync()
        {
            string chatRoomId = Context.GetHttpContext().Request.Query["chatRoomId"].SingleOrDefault();
            string token = Context.GetHttpContext().Request.Query["token"].SingleOrDefault();

            string chatRoom;
            if (!_connectionIds.TryGetValue(token + chatRoomId, out chatRoom) || chatRoom != chatRoomId)
            {
                _connectionIds.Add(token + chatRoomId, chatRoomId);

                await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string chatRoomId = Context.GetHttpContext().Request.Query["chatRoomId"].SingleOrDefault();

            _connectionIds.Remove(chatRoomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string chatRoomId, string loginId, string message)
        {
            var createdAt = DateTime.UtcNow;

            var sender = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();

            await Clients.Group(chatRoomId).SendAsync("displayMessage", sender.Id, message, createdAt.ToString("O"));
           
            await _chatService.CreateNewMessage(chatRoomId, sender, message, createdAt);
        }
        
        //----------------------------------ConnectionTest-----------------------------------

        public Task BroadcastMessage(string name, string message) =>
            Clients.All.SendAsync("broadcastMessage", name, message);

        public Task Echo(string name, string message) =>
            Clients.Client(Context.ConnectionId)
                   .SendAsync("echo", name, $"{message} (echo from server)");
    }
}
