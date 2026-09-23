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

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ImageUploadResult();

            using var stream = file.OpenReadStream();

            // PDF
            if (file.ContentType == "application/pdf")
            {
                var pdfParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
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
                File = new FileDescription(file.FileName, stream),
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