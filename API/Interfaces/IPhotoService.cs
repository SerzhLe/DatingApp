using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;

namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file); //IFormFile - represents a file sent with http request
        Task<DeletionResult> DeleteImageAsync(string publicId); //each image will have a public id
    }
}