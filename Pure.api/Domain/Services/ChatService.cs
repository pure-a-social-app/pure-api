using MongoDB.Bson;
using Pure.api.Domain.Contracts;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Services
{
    public class ChatService : IChatService
    {
        private IRepository<User> _userRepository;
        private IRepository<ChatRoom> _chatRoomRepository;
        private IFileService _fileService;

        public ChatService(IRepository<User> userRepository, IRepository<ChatRoom> chatRoomRepository,
            IFileService fileService)
        {
            _userRepository = userRepository;
            _chatRoomRepository = chatRoomRepository;
            _fileService = fileService;
        }

        public Task<ChatMessage> CreateNewAttachmentMessage(string conversationId, string senderId, string fileName, string contentType, string attachmentType, string referer, string refererUrl, string recipientId)
        {
            return null;
        }

        public async Task CreateNewMessage(string chatRoomId, User sender, string content, DateTime createdAt)
        {
            var chatRoom = (await _chatRoomRepository.Get(x => x.Id == chatRoomId)).FirstOrDefault();

            var newMessage = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Content = content,
                CreatedById = sender.Id,
                SentOn = createdAt
            };

            chatRoom.Messages.Insert(0, newMessage);

            await _chatRoomRepository.Update(chatRoom);
        }

        public Task<IEnumerable<ChatMessage>> GetAllInitially(string conversationId)
        {
            throw new NotImplementedException();
        }
    }
}
