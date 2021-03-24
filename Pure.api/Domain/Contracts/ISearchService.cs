using Pure.api.Domain.Models.Search;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface ISearchService
    {
        Task<List<User>> GetUserAvatarURLs(List<User> users);
        Task<List<SearchPeopleViewModel>> SearchUsersByName(string userName);
    }
}
