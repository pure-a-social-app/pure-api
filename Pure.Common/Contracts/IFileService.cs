using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Pure.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pure.Common.Contracts
{
    public interface IFileService
    {
        Task<string> SaveFileToS3(IFormFile file, FileType type);
        Task<Attachment> UploadAttachment(IFormFile photo);
        Task<bool> DeleteFile(string id, FileType type);
        Task<string> GetFileURL(string id, FileType type);
        List<string> GetAllVideos<T>(List<T> list);
        Task<string> GetImagePrefix(string key, FileType type);
        Task<List<S3Object>> GetAllFiles(FileType type);
        Task<List<string>> GetPostImagePrefixes(List<string> names);
        string GetMessageAttachmentPrefix(string blobName);
        Task<List<string>> GetOnlineShoppingImagePrefixes(List<string> names);
    }

    public enum FileType
    {
        PostImage = 1,
        MessageAttachment,
        ShoppingItem
    }
}
