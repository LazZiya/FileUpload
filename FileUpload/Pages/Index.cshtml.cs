using FileUpload.Utilities;
using LazZiya.ImageResize;
using LazZiya.ImageResize.Watermark;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FileUpload.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        [ValidateAntiForgeryToken]
        public async Task OnPostAsync(List<IFormFile> files)
        {
            foreach (var file in Request.Form.Files)
            {
                //get uploaded file name: true to create temp name, false to get real name
                var fileName = file.TempFileName(false);

                if (file.Length > 0)
                {
                    // optional : server side resize create image with watermark
                    // these steps requires LazZiya.ImageResize package from nuget.org
                    if (fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".png"))
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            // create image file from uploaded file stream
                            // also possible to create image from file Image.FromFile("....")
                            var img = Image.FromStream(stream);

                            // resize to new image sizes (800x600)
                            var newImg = ImageResize.ScaleAndCrop(img, 800, 400);

                            // add text watermark to new image
                            newImg.TextWatermark("http://Ziyad.info");

                            // add image watermark to new image
                            newImg.ImageWatermark("wwwroot\\images\\icon.png");

                            _logger.LogInformation($"----> wwwroot\\upload\\{fileName}");
                            // save as new image
                            newImg.SaveAs($"wwwroot\\upload\\{fileName}");

                            img.Dispose();
                            newImg.Dispose();
                        }
                    }
                    else
                    {
                        // upload and save files to upload folder
                        using (var stream = new FileStream($"wwwroot\\upload\\{fileName}", FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }
        }

        public JsonResult OnGetListFolderContents()
        {
            var folderPath = $"wwwroot\\upload";

            if (!Directory.Exists(folderPath))
                return new JsonResult("Folder not exists!") { StatusCode = (int)HttpStatusCode.NotFound };

            var folderItems = Directory.GetFiles(folderPath);

            if (folderItems.Length == 0)
                return new JsonResult("Folder is empty!") { StatusCode = (int)HttpStatusCode.NoContent };

            var galleryItems = new List<FileItem>();

            foreach (var file in folderItems)
            {
                var fileInfo = new FileInfo(file);
                galleryItems.Add(new FileItem
                {
                    Name = fileInfo.Name,
                    FilePath = $"https://localhost:44326/upload/{fileInfo.Name}",
                    FileSize = fileInfo.Length
                });
            }

            return new JsonResult(galleryItems) { StatusCode = 200 };
        }

        public JsonResult OnGetDeleteFile(string file)
        {
            var filePath = Path.Combine($"wwwroot\\upload\\{file}");

            try
            {
                System.IO.File.Delete(filePath);
            }
            catch
            {
                return new JsonResult(false) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }

            return new JsonResult(true) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
