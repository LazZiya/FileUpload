using FileUpload.Utilities;
using LazZiya.ImageResize;
using LazZiya.ImageResize.Watermark;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
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
                            // Create image file from uploaded file stream
                            // Then resize, and add text/image watermarks
                            // And save
                            using (var img = Image.FromStream(stream))
                            {
                                img.ScaleAndCrop(300, 300, TargetSpot.MiddleRight)
                                .AddImageWatermark(@"wwwroot\images\icon.png")
                                .AddTextWatermark("http://ziyad.info")
                                .SaveAs($"wwwroot\\upload\\{fileName}");
                            }
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
