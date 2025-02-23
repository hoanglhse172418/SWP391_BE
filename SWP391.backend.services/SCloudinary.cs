using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SWP391.backend.services
{
    public class SCloudinary
    {
        private readonly Cloudinary _cloudinary;
        public SCloudinary(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = Path.GetFileNameWithoutExtension(file.FileName),
                Overwrite = true,
                Folder = "vaccine-images"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString(); // Return Cloudinary image URL
        }
    }
}
