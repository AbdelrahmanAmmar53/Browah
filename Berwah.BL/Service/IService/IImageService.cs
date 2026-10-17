using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);
        Task<ImageUploadResult> UploadImageAsync(byte[] fileBytes, string fileName, string contentType);
        Task<DeletionResult> DeleteImageAsync(string publicId);
    }
}
