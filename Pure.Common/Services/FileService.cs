using Pure.Common.Aws;
using Pure.Common.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Pure.Common.Models;
using MongoDB.Bson;

namespace Pure.Common.Services
{
    public class FileService : IFileService
    {
        private readonly string _tempFolder = "\\Images\\";
        private IAwsService _awsService;
        private IWebHostEnvironment _webHostEnvironment;

        public static readonly string POST_PHOTO_BUCKET_NAME = "pure-post";
        public static readonly string MESSAGE_PHOTO_BUCKET_NAME = "pure-message";
        public static readonly string SHOPPING_ITEM_BUCKET_NAME = "pure-file";

        public FileService(IAwsService awsService, IWebHostEnvironment webHostEnvironment)
        {
            _awsService = awsService;
            _webHostEnvironment = webHostEnvironment;
        }

        public Task<bool> DeleteFile(string id, FileType type)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllVideos<T>(List<T> list)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFileURL(string id, FileType type)
        {
            throw new NotImplementedException();
        }

        public string GetMessageAttachmentPrefix(string blobName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetOnlineShoppingImagePrefixes(List<string> names)
        {
            return await _awsService.GetPresignedUrlObject(names, SHOPPING_ITEM_BUCKET_NAME);
        }

        public async Task<List<string>> GetPostImagePrefixes(List<string> names)
        {
            return await _awsService.GetPresignedUrlObject(names, POST_PHOTO_BUCKET_NAME);
        }

        public async Task<string> GetImagePrefix(string key, FileType type)
        {
            if (key == null)
                return null;

            switch (type)
            {
                case FileType.PostImage:
                    return await _awsService.GetPresignedUrlObject(key, POST_PHOTO_BUCKET_NAME);
                case FileType.MessageAttachment:
                    return await _awsService.GetPresignedUrlObject(key, MESSAGE_PHOTO_BUCKET_NAME);
                case FileType.ShoppingItem:
                    return await _awsService.GetPresignedUrlObject(key, SHOPPING_ITEM_BUCKET_NAME);
                default:
                    return null;
            }
        }

        public async Task<List<S3Object>> GetAllFiles(FileType type)
        {
            try
            {
                switch (type)
                {
                    case FileType.PostImage:
                        return await _awsService.GetAllObjectFromS3(POST_PHOTO_BUCKET_NAME);
                    case FileType.MessageAttachment:
                        return await _awsService.GetAllObjectFromS3(MESSAGE_PHOTO_BUCKET_NAME);
                    case FileType.ShoppingItem:
                        return await _awsService.GetAllObjectFromS3(SHOPPING_ITEM_BUCKET_NAME);                        
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Attachment> UploadAttachment(IFormFile photo)
        {
            if (photo == null)
            { 
                return null;
            }

            string key = await SaveFileToS3(photo, FileType.PostImage);
            var attachment = new Attachment
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Key = key,
                FileName = photo.FileName
            };

            return attachment;
        }

        public async Task<string> SaveFileToS3(IFormFile file, FileType type)
        {
            try
            {
                switch (type)
                {
                    case FileType.PostImage:
                        return await _awsService.PutFileToS3(file.FileName, file.OpenReadStream(), POST_PHOTO_BUCKET_NAME);
                    case FileType.MessageAttachment:
                        return await _awsService.PutFileToS3(file.FileName, file.OpenReadStream(), MESSAGE_PHOTO_BUCKET_NAME);
                    case FileType.ShoppingItem:
                        return await _awsService.PutFileToS3(file.FileName, file.OpenReadStream(), SHOPPING_ITEM_BUCKET_NAME);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool CheckValidPhotoExtensionAndLength(IFormFile photo)
        {
            var fileExtension = Path.GetExtension(photo.FileName);
            List<string> acceptedExtensions = new List<string> { ".jpg", ".png", ".gif", ".jpeg" };

            if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
            {
                return false;
            }

            if (photo.Length > 0)
            {
                return false;
            }

            return true;
        }

        public void SaveFileToLocal(IList<IFormFile> photos)
        {
            try
            {
                foreach (var photo in photos)
                {
                    if (CheckValidPhotoExtensionAndLength(photo))
                    {
                        if (!Directory.Exists(_webHostEnvironment.WebRootPath + _tempFolder))
                        {
                            Directory.CreateDirectory(_webHostEnvironment.WebRootPath + _tempFolder);
                        }

                        using (FileStream fileStream = File.Create(_webHostEnvironment.WebRootPath + _tempFolder + photo.FileName))
                        {
                            photo.CopyTo(fileStream);
                            fileStream.Flush();
                        };
                    }
                    else
                    {
                        throw new Exception("Photo is invalid!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
    }
}
