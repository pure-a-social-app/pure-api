using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Search;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Services
{
    public class SearchService : ISearchService
    {
        private IRepository<User> _userRepository;
        private IFileService _fileService;

        public SearchService(IRepository<User> userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        public async Task<List<SearchPeopleViewModel>> SearchUsersByName(string userName)
        {
            userName = userName.ToLower();
            var users = (await _userRepository.FindAsync(x => x.UserName.ToLower().Contains(userName))).ToList();
            var avatarUsers = (await GetUserAvatarURLs(users)).ToList();
            var userList = new List<SearchPeopleViewModel>();

            foreach (var user in avatarUsers)
            {
                userList.Add(new SearchPeopleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    WalletAddress = user.WalletAddress,
                    Avatar = user.Avatar != null ? user.Avatar : null
                });
            }

            return userList;
        }

        public async Task<List<User>> GetUserAvatarURLs(List<User> users)
        {
            foreach (var user in users)
            {
                if (user.Avatar != null)
                {
                    user.Avatar.AttachmentUrl = await _fileService.GetImagePrefix(user.Avatar.Key, FileType.PostImage);
                }
            }

            return users;
        }
    }
}
