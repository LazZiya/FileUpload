using Microsoft.AspNetCore.Http;
using System.IO;

namespace FileUpload.Utilities
{
    public static class IFileNameExtension
    {
        public static string TempFileName(this IFormFile file, bool randomName = true)
        {
            var ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));

            var fileName = randomName ? Path.GetTempFileName() : file.FileName;

            var start = fileName.LastIndexOf('\\');
            var end = fileName.LastIndexOf('.');

            return fileName.Substring(start, end - start) + ext;
        }
    }
}
