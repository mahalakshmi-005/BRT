using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BRT.Services
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    // Cloudinary-backed implementation of IFileUploadService — swap this in for FileUploadService
    // so uploaded product/category images live in Cloudinary instead of the container's local
    // (ephemeral, Render-wiped-on-redeploy) disk.
    public class CloudinaryFileUploadService : IFileUploadService
    {
        private readonly Cloudinary _cloudinary;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public CloudinaryFileUploadService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("Only JPG, PNG, WEBP, or GIF images are allowed.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("Image must be smaller than 5 MB.");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"brt/{subFolder}",
                PublicId = Guid.NewGuid().ToString("N"),
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");

            // Store the full Cloudinary URL directly in Product/Category.ImageUrl — views already
            // render ImageUrl as-is (<img src="@p.ImageUrl">), so no view changes are needed.
            return result.SecureUrl?.ToString();
        }

        public void DeleteImage(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.Contains("res.cloudinary.com")) return;

            // Extract the public ID (folder/filename without extension) from the Cloudinary URL
            // e.g. https://res.cloudinary.com/<cloud>/image/upload/v123/brt/products/abc123.jpg
            var uploadIndex = relativeUrl.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex < 0) return;

            var afterUpload = relativeUrl[(uploadIndex + "/upload/".Length)..];
            var slashIndex = afterUpload.IndexOf('/');
            var pathPart = slashIndex >= 0 ? afterUpload[(slashIndex + 1)..] : afterUpload; // strip version segment (v123456/)
            var publicId = Path.ChangeExtension(pathPart, null);

            if (string.IsNullOrWhiteSpace(publicId)) return;

            _cloudinary.Destroy(new DeletionParams(publicId));
        }
    }
}

