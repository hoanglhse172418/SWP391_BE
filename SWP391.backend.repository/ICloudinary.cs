using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace SWP391.backend.repository
{
    public interface ICloudinary
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
