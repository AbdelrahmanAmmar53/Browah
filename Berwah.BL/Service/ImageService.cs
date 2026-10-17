using Berwah.BL.Service.IService;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class ImageService: IImageService
    {
        private readonly Cloudinary _cloudinary;


        public ImageService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account
            (
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        // Standard IFormFile Upload
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ImageUploadResult();

            using var stream = file.OpenReadStream();
            return await UploadFromStreamAsync(stream, file.FileName, file.ContentType);
        }

        // Overload for FileDataDto / Byte Array (Used in OTP Registration flow)
        public async Task<ImageUploadResult> UploadImageAsync(byte[] fileBytes, string fileName, string contentType)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                return new ImageUploadResult();

            using var stream = new MemoryStream(fileBytes);
            return await UploadFromStreamAsync(stream, fileName, contentType);
        }

        // Private helper to remove duplicate Cloudinary code
        private async Task<ImageUploadResult> UploadFromStreamAsync(Stream stream, string fileName, string contentType)
        {
            // PDF
            if (contentType == "application/pdf")
            {
                var pdfParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = "da-net7"
                };

                var pdfResult = await _cloudinary.UploadAsync(pdfParams);

                return new ImageUploadResult
                {
                    PublicId = pdfResult.PublicId,
                    SecureUrl = pdfResult.SecureUrl
                };
            }

            // Images
            var imageParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Transformation = new Transformation(),
                Folder = "da-net7"
            };

            return await _cloudinary.UploadAsync(imageParams);
        }
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);

            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}