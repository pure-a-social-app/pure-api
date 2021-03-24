using Pure.Common.Contracts;
using Pure.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Pure.Common.Services
{
    public class EmailService : IEmailService
    {
        private static string Username = "mvp.billion@gmail.com";
        private static string Password = "billion2017";
        private static bool EnableSsl = true;
        private static string Host = "smtp.gmail.com";
        private static readonly int Port = 587;
        private static string SendGridApiKey = "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";

        private static SmtpClient Client = new SmtpClient()
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(Username, Password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = EnableSsl,
            Host = Host,
            Port = Port,
        };

        public async Task SendAsync(EmailMessage message, string templateId, Dictionary<string, string> presetSubstitutions)
        {
            await ConfigSendGridAsync(message, templateId, presetSubstitutions);
        }

        public async Task SendAsync(ICollection<EmailMessage> messages, string templateId)
        {
            //await ConfigSendGridAsync(messages, templateId);
        }

        public Task<HttpStatusCode> ConfigSendGridAsync(EmailMessage message, string templateId, Dictionary<string, string> presetSubstitutions)
        {
            message.PresetSubstitutions = presetSubstitutions;
            return null;
            //return ConfigSendGridAsync(new List<EmailMessage> { message }, templateId);
        }

        async public Task<HttpStatusCode> ConfigSendGridAsync(ICollection<EmailMessage> messages, string templateId)
        {
            //            var client = new SendGridClient(SendGridApiKey);

            //            var sendGridMsg = new SendGridMessageRequest();
            //            sendGridMsg.Template_id = templateId;
            //            sendGridMsg.From = new Recipient { Email = "noreply@globaltalent.io", Name = "Talent - MVPStudio.IO" };
            //            sendGridMsg.Personalizations = new List<Personalization>
            //            {
            //                new Personalization
            //                {
            //                    Subject = message.Subject,
            //                    To = new List<Recipient>
            //                    {
            //                        new Recipient
            //                        {
            //                            Email = message.Destination,
            //                            Name = message.FirstName
            //                        },
            //                    },
            //                    Substitutions = message.PresetSubstitutions
            //                };

            //            sendGridMsg.Personalizations.Add(personalisation);
            //        };

            //        sendGridMsg.Content = new List<Content>()
            //            {
            //                new Content
            //                {
            //                    Value = "Hello, Email!",
            //                    Type = "text/html"
            //                }
            //};
            //var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(sendGridMsg).ToString();
            //var response = await client.RequestAsync(method: SendGridClient.Method.POST,
            //                                             requestBody: jsonString,
            //                                             urlPath: "mail/send");
            return HttpStatusCode.OK;
        }

        //Will be needed for creating template//
        public class SendGridMessageRequest
        {
            [JsonProperty("template_id")]
            public string Template_id { get; set; }
            [JsonProperty("from")]
            public Recipient From { get; set; }
            [JsonProperty("personalizations")]
            public List<Personalization> Personalizations { get; set; }
            [JsonProperty("content")]
            public List<Content> Content { get; set; }
        }

        public class Recipient
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        public class Personalization
        {
            public Dictionary<string, string> Substitutions { get; set; }
            public List<Recipient> To { get; set; }
            public List<Recipient> Cc { get; set; }
            public List<Recipient> Bcc { get; set; }
            public string Subject { get; set; }
        }

        public class SendGridBaseRequest
        {
            public string EmailAddress { get; set; }
            public string FullName { get; set; }
            public string Subject { get; set; }
        }

        public class SendGridPostScheduleRequest : SendGridBaseRequest
        {
            public string PostSubject { get; set; }
            public string PostBody { get; set; }
            public string PostUrl { get; set; }
        }

        public class Content
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
