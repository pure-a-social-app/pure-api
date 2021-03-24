using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Shopping;
using Pure.Common.Contracts;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pure.api.Domain.Services
{
    public class ShoppingService : IShoppingService
    {
        private IRepository<User> _userRepository;
        private IRepository<ShoppingItem> _shoppingItemRepository;
        private IRepository<ShoppingSeller> _shoppingSellerRepository;
        private IFileService _fileService;

        public ShoppingService(IRepository<User> userRepository, 
            IRepository<ShoppingItem> shoppingItemRepository,
            IRepository<ShoppingSeller> shoppingSellerRepository,
            IFileService fileService)
        {
            _userRepository = userRepository;
            _shoppingItemRepository = shoppingItemRepository;
            _shoppingSellerRepository = shoppingSellerRepository;
            _fileService = fileService;
        }

        public async Task<bool> CreateNewShop(string loginId, string shopAddress)
        {
            var user = (await _userRepository.FindAsync(x => x.Login.Id == loginId)).FirstOrDefault();
            
            if (user.ShopAddress != null)
            {
                return false;
            }

            user.ShopAddress = shopAddress;
            await _userRepository.Update(user);

            return true;
        }

        public async Task<ShoppingItem> AddItem(IList<IFormFile> photos, string details)
        {
            var itemDetails = JsonConvert.DeserializeObject<ShoppingItemDetailViewModel>(details);

            var user = (await _userRepository.FindAsync(x => x.Login.Id == itemDetails.LoginId)).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // Add photos to AWS and get their key
            var photoKeys = new List<Attachment>();
            for (int i = 0; i < photos.Count; i++)
            {
                string key = await _fileService.SaveFileToS3(photos[i], FileType.ShoppingItem);
                photoKeys.Add(new Attachment
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Key = key,
                    FileName = photos[i].FileName
                });
            }

            var seller = (await _shoppingSellerRepository.FindAsync(x => x.UId == user.UId)).FirstOrDefault();
            
            bool sellerIsCreated = true;
            if (seller == null)
            {
                seller = new ShoppingSeller
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UId = user.UId,
                    UserId = user.Id,
                    ShopAddress = user.ShopAddress,
                    ShoppingItems = new List<ShoppingItem>(),
                    IsDeleted = false
                };

                sellerIsCreated = false;
            }

            ShoppingItem item = new ShoppingItem
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UId = Guid.NewGuid(),
                SellerUserId = seller.Id,
                ShopAddress = seller.ShopAddress,
                Name = itemDetails.Name,
                Price = itemDetails.Price,
                Stock = itemDetails.Stock,
                Description = itemDetails.Description,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsSold = false,
                IsReceived = false,
                Attachments = photoKeys
            };

            seller.ShoppingItems.Add(item);

            if (sellerIsCreated)
            {
                await _shoppingSellerRepository.Update(seller);
            }
            else
            {
                await _shoppingItemRepository.Add(item);
            }

            await GetItemImagePrefixes(item);

            return item;
        }

        public async Task<List<ShoppingItem>> GetAllFiles(FileType type)
        {
            List<ShoppingItem> itemDetails = await _shoppingItemRepository.FindAllAsyncReversed();

            foreach (var item in itemDetails)
            {
                await GetItemImagePrefixes(item);
            }

            return itemDetails;
        }

        public async Task<List<ShoppingItem>> GetShopItems(string sellerId)
        {
            var seller = (await _shoppingSellerRepository.FindAsync(x => x.Id == sellerId)).FirstOrDefault();

            // Get all images sold by the seller from AWS 

            var reversedItems = seller.ShoppingItems;
            reversedItems.Reverse();

            return reversedItems;
        }

        public async Task<List<string>> GetItemImagePrefixes(ShoppingItem item)
        {
            var keys = item.Attachments.Select(x => x.Key).ToList();
            var imagePrefixes = await _fileService.GetOnlineShoppingImagePrefixes(keys);

            for (int i = 0; i < imagePrefixes.Count; i++)
            {
                item.Attachments[i].AttachmentUrl = imagePrefixes[i];
            }

            return imagePrefixes;
        }
    }
}
