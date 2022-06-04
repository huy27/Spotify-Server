using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Application.IService;
using Data.Entities;

namespace Application.Ultilities
{
    public static class FileService
    {
        public static async Task SaveFile(string jsonData, string filename)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}");
                await File.WriteAllTextAsync(filePath, jsonData);
                
            }
            catch (Exception ex)
            {

            }
        }

        public static string GetUrl(string filename)
        {
            var _userContentFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}");
            return _userContentFolder;
        }
    }
}
