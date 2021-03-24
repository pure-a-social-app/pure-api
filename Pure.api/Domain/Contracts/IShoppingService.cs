using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Pure.api.Domain.Models.Shopping;
using Pure.Common.Commands;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pure.api.Domain.Contracts
{
    public interface IShoppingService
    {
        Task<bool> CreateNewShop(string loginId, string shopAddress);
        Task<ShoppingItem> AddItem(IList<IFormFile> photos, string itemDetails);
        Task<List<ShoppingItem>> GetAllFiles(FileType type);
        Task<List<ShoppingItem>> GetShopItems(string sellerId);
    }
}
