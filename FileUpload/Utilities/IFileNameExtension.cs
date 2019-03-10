using Microsoft.AspNetCore.Http;
using System.IO;

namespace FileUpload.Utilities
{
    public static class IFileNameExtension
    {
        /// <summary>
        /// create temporary file name or return file name from upload stream
        /// </summary>
        /// <param name="file">uploaded file</param>
        /// <param name="randomName">true to return temp name, false to return uploaded file name</param>
        /// <returns></returns>
        public static string TempFileName(this IFormFile file, bool randomName = true)
        {
            var ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));

            var fileName = randomName ? Path.GetTempFileName() : file.FileName;

            var start = fileName.LastIndexOf('\\');

            if (start < 0)
                start = 0;

            var end = fileName.LastIndexOf('.');

            return $"{fileName.Substring(start, end - start)}{ext}".TrimStart('\\');
        }
    }
}
