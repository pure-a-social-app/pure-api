using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IChatService
    {
        Task<IEnumerable<ChatMessage>> GetAllInitially(string conversationId);
        Task CreateNewMessage(
            string chatRoomId,
            User sender,
            string content,
            DateTime createdAt);
        Task<ChatMessage> CreateNewAttachmentMessage(
            string conversationId,
            string senderId,
            string fileName,
            string contentType,
            string attachmentType,
            string referer,
            string refererUrl,
            string recipientId);
    }
}
