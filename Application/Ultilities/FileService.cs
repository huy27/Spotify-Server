using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Application.IService;
using Data.Entities;
using Data.Enums;

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

        public static async Task SaveCsvFile<T>(List<T> listData, string filename, TypeCsvFile typeCsvFile)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}");
            StringBuilder csv = new StringBuilder();

            switch (typeCsvFile)
            {
                case TypeCsvFile.Album:
                    csv.AppendLine(string.Join(",", "Id", "Name", "Description", "Createdate"));

                    foreach (T item in listData)
                    {
                        var type = typeof(T);
                        var id = type.GetProperty("Id").GetValue(item).ToString();
                        var name = HandleCommaCsv(type.GetProperty("Name").GetValue(item).ToString());
                        var description = HandleCommaCsv(type.GetProperty("Description").GetValue(item).ToString());
                        var createdate = type.GetProperty("CreatedAt").GetValue(item).ToString();

                        csv.AppendLine(string.Join(",", id, name, description, createdate));
                    }
                    break;
                case TypeCsvFile.Music:
                    csv.AppendLine(string.Join(",", "Id", "Name", "Author", "Url", "Lyric", "CreateDate"));

                    foreach (T item in listData)
                    {
                        var type = typeof(T);
                        var id = type.GetProperty("Id").GetValue(item).ToString();
                        var name = HandleCommaCsv(type.GetProperty("Name").GetValue(item).ToString());
                        var author = HandleCommaCsv(type.GetProperty("Author").GetValue(item).ToString());
                        var url = HandleCommaCsv(type.GetProperty("Url").GetValue(item).ToString());
                        var lyric = HandleCommaCsv(type.GetProperty("Lyric").GetValue(item).ToString());
                        var createDate = type.GetProperty("CreateDate").GetValue(item)?.ToString();

                        csv.AppendLine(string.Join(",", id, name, author, url, lyric, createDate));
                    }
                    break;
                default:
                    break;
            }

            csv.AppendLine(String.Join(",", "Total", listData.Count));

            await File.WriteAllTextAsync(filePath, csv.ToString());
        }

        private static string HandleCommaCsv(string data)
        {
            if (data.Contains(","))
                return "\"" + data + "\"";

            return data;
        }
    }
}
