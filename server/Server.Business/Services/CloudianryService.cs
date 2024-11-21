using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Server.Business.Dtos;
using Server.Business.Ultils;

namespace Server.Business.Services
{
    public class CloudianryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudianryService(IOptions<CloundSettings> cloudinaryConfig)
        {
            var account = new Account(
                cloudinaryConfig.Value.CloudName,
                cloudinaryConfig.Value.CloudKey,
                cloudinaryConfig.Value.CloudSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = Path.GetFileNameWithoutExtension(file.FileName),
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }
    }
}
