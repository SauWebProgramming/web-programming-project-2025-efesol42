namespace BendenSana.Repositories
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        void DeleteImage(string? imageUrl);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/{folderName}/" + uniqueFileName;
        }

        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(imagePath)) File.Delete(imagePath);
        }
    }
}
