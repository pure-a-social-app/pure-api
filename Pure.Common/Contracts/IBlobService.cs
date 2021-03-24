using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Pure.Common.Contracts
{
    public interface IBlobService
    {
        Task<string> SaveProfileImageAsync(IFormFile image);
        Task<string> SaveFileAsync(IFormFile image, FileType fileType);
        string GetBlobUrlAsync(FileType fileType);
    }
}
