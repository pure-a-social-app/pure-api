using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pure.Common.Aws
{
    public interface IAwsService
    {
        Task<List<S3Object>> GetAllObjectFromS3(string bucketName);
        Task<S3Object> GetObjectFromName(string name, string bucketName);
        Task<string> GetStaticUrl(string name, string bucketName);
        Task<string> GetPresignedUrlObject(string name, string bucketName);
        Task<List<string>> GetPresignedUrlObject<T>(List<T> objectNames, string bucketName);
        Task<string> PutFileToS3(string name, Stream stream, string bucketName, bool isPublic = false);
        Task<bool> RemoveFileFromS3(string name, string bucketName);
    }
}
