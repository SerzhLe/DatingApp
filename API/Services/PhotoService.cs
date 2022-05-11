using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloud;

        public PhotoService(IOptions<CloudinarySettings> config) //когда нужно добраться до CloudinarySettings
        {
            var account = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            //get an account with default instance of config

            _cloud = new Cloudinary(account);
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);

            var result = await _cloud.DestroyAsync(deletionParams);

            return result;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream(); //open stream to read the file in the future

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face") //500 px
                    //this transformation will make image square 500x500 and focus on face
                };

                uploadResult = await _cloud.UploadAsync(uploadParams);
            }

            return uploadResult;
        }
    }
}