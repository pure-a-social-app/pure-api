using MongoDB.Bson;
using Pure.api.Domain.Contracts;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pure.api.Domain.Models.Message;

namespace Pure.api.Domain.Services
{
    public class MessageService : IMessageService
    {
        private IRepository<User> _userRepository;
        private IRepository<ChatRoom> _chatRoomRepository;
        private IFileService _fileService;

        public MessageService(IRepository<User> userRepository,
            IRepository<ChatRoom> chatRoomRepository, 
            IFileService fileService)
        {
            _userRepository = userRepository;
            _chatRoomRepository = chatRoomRepository;
            _fileService = fileService;
        }

        public async Task<List<UserChatRoom>> GetUserChatList(string loginId)
        {
            var user = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();

            var chatList = (await _chatRoomRepository
                .FindAsync(x => x.FriendId == user.Id || x.CreatorId == user.Id)).ToList();
            chatList.OrderBy(x => x.Messages[0].SentOn);

            var chatRooms = new List<UserChatRoom>();
            foreach (var chatRoom in chatList)
            {
                string friendId = user.Id == chatRoom.CreatorId ? chatRoom.FriendId : chatRoom.CreatorId;
                var friend = (await _userRepository.FindAsync(x => x.Id == friendId)).FirstOrDefault();
                
                if (friend.Avatar != null)
                {
                    friend.Avatar.AttachmentUrl = await _fileService.GetImagePrefix(friend.Avatar.Key, FileType.PostImage);
                }

                chatRooms.Add(new UserChatRoom
                {
                    ChatRoomId = chatRoom.Id,
                    LastMessage = chatRoom.Messages.Count > 0 ? chatRoom.Messages[0].Content : null,
                    UserId = friend.Id,
                    UserName =  friend.UserName,
                    Avatar = friend.Avatar ?? null,
                    UserWalletAddress = friend.WalletAddress
                });
            }

            return chatRooms;
        }

        public async Task<ChatRoom> GetRoomMessages(GetRoomMessageViewModel vm)
        {
            var user = (await _userRepository.FindAsync(x => x.Login.Id == vm.LoginId)).FirstOrDefault();

            string creatorUrl = null;
            if (user.Avatar != null)
            {
                creatorUrl = await _fileService.GetImagePrefix(user.Avatar.Key, FileType.PostImage);
            }

            string friendUrl = null;
            if (vm.FriendAvatarKey != null)
            {
                friendUrl = await _fileService.GetImagePrefix(vm.FriendAvatarKey, FileType.PostImage);
            }

            var chatRoom = await GetChatRoomFromId(user.Id, vm.FriendId);
            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    CreatorId = user.Id,
                    Creator = user.UserName,
                    CreatorAvatarUrl = creatorUrl,
                    FriendId = vm.FriendId,
                    FriendName = vm.FriendName,
                    FriendAvatarUrl = friendUrl,
                    IsDeleted = false,
                    Messages = new List<ChatMessage>(),
                    FriendWalletAddress = vm.FriendWalletAddress
                };
                
                await _chatRoomRepository.Add(chatRoom);
            }
            else if (chatRoom.CreatorId == user.Id)
            {
                chatRoom.CreatorAvatarUrl = creatorUrl;
                chatRoom.FriendAvatarUrl = friendUrl;
            }
            else
            {
                chatRoom.CreatorAvatarUrl = friendUrl;
                chatRoom.FriendAvatarUrl = creatorUrl;
            }

            return chatRoom;
        }

        public async Task<ChatRoom> GetChatRoomFromId(string userId, string friendId)
        {
            return (await _chatRoomRepository.FindAsync(x => 
                (x.CreatorId == userId && x.FriendId == friendId) 
                || (x.CreatorId == friendId && x.FriendId == userId)))
                .FirstOrDefault();
        }
    }
}
