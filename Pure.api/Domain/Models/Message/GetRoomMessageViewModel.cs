using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Models.Message
{
    public class GetRoomMessageViewModel
    {
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string FriendId { get; set; }
        [Required]
        public string FriendName { get; set; }
        [Required]
        public string FriendWalletAddress { get; set; }
        public string FriendAvatarKey { get; set; }
    }
}
