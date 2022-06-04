using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Application.IService;
using Data.Entities;

namespace Application.Ultilities
{
    public static class File
    {
        public static void SaveFile(string jsonData, string filename)
        {
            var _userContentFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}.json");
            System.IO.File.WriteAllText(_userContentFolder, jsonData);
        }
    }
}
