using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Server.Business.Exceptions;
using Server.Business.Ultils;

namespace Server.Business.Services
{
    public class CloudService
    {
        private readonly Cloudinary _cloudinary;

        public CloudService(IOptions<CloundSettings> cloundSettingsOptions)
        {
            var cloudSettings = cloundSettingsOptions.Value;

            Account account = new Account(
                cloudSettings.CloundName,
                cloudSettings.CloundKey,
                cloudSettings.CloundSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("File is null or empty.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        UploadPreset = "LandL"
                    };

                    // Tải lên không đồng bộ
                    return await Task.Run(() => _cloudinary.UploadAsync(uploadParams));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload image to Cloudinary.", ex);
            }
        }




        public async Task<List<ImageUploadResult>> UploadImagesAsync(List<IFormFile> files)
        {
            var uploadResults = new List<ImageUploadResult>();

            foreach (var file in files)
            {
                uploadResults.Add(await UploadImageAsync(file));
            }

            return uploadResults;
        }
    }
}
