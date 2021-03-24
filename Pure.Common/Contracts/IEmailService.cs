using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Pure.Common.Contracts
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message, string templateId, Dictionary<string, string> presetSubstitutions);
        Task SendAsync(ICollection<EmailMessage> messages, string templateId);
        Task<HttpStatusCode> ConfigSendGridAsync(EmailMessage message, string templateId, Dictionary<string, string> presetSubstitutions);
        Task<HttpStatusCode> ConfigSendGridAsync(ICollection<EmailMessage> messages, string templateId);
    }
}
