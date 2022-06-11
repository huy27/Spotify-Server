using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Application.IService;
using Aspose.Pdf;
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

            if (!File.Exists(_userContentFolder))
                return "";

            return _userContentFolder;
        }

        public static async Task SaveCsvFile<T>(List<T> listData, string filename, EnumFile typeCsvFile)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}");
            StringBuilder csv = new StringBuilder();

            switch (typeCsvFile)
            {
                case EnumFile.Album:
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
                case EnumFile.Music:
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

        public static void SavePdfFile<T>(List<T> data, string filename, EnumFile typePdfFile)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content", $"{filename}.pdf");
                Document document = new Document();

                Page page = document.Pages.Add();
                Table table = new Table();
                table.Border = new BorderInfo(BorderSide.All, .5f, Color.FromRgb(System.Drawing.Color.LightGray));
                table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Color.FromRgb(System.Drawing.Color.LightGray));

                MarginInfo margin = new MarginInfo();
                margin.Top = 1;
                margin.Left = 1;
                margin.Right = 1;
                margin.Bottom = 1;

                table.DefaultCellPadding = margin;
                Row header = table.Rows.Add();
                switch (typePdfFile)
                {
                    case EnumFile.Album:
                        table.ColumnWidths = "30 100 165 125";

                        header.Cells.Add("Id");
                        header.Cells.Add("Name");
                        header.Cells.Add("Description");
                        header.Cells.Add("CreatedAt");

                        for (int i = 0; i < data.Count; i++)
                        {
                            Row row = table.Rows.Add();

                            var type = typeof(T);
                            var id = type.GetProperty("Id").GetValue(data[i]).ToString();
                            var name = type.GetProperty("Name").GetValue(data[i]).ToString();
                            var description = type.GetProperty("Description").GetValue(data[i]).ToString();
                            var createdate = type.GetProperty("CreatedAt").GetValue(data[i]).ToString();

                            row.Cells.Add(id);
                            row.Cells.Add(name);
                            row.Cells.Add(description);
                            row.Cells.Add(createdate);
                        }
                        break;
                    case EnumFile.Music:
                        table.ColumnWidths = "30 100 90 100 100";

                        header.Cells.Add("Id");
                        header.Cells.Add("Name");
                        header.Cells.Add("Author");
                        header.Cells.Add("CreateDate");
                        header.Cells.Add("Image");

                        for (int i = 0; i < data.Count; i++)
                        {
                            Row row = table.Rows.Add();
                            Image img = new Image();

                            var type = typeof(T);
                            var id = type.GetProperty("Id").GetValue(data[i]).ToString();
                            var name = HandleCommaCsv(type.GetProperty("Name").GetValue(data[i]).ToString());
                            var author = HandleCommaCsv(type.GetProperty("Author").GetValue(data[i]).ToString());
                            var createDate = type.GetProperty("CreateDate").GetValue(data[i])?.ToString();
                            img.File = type.GetProperty("Image").GetValue(data[i])?.ToString();
                            img.FixWidth = 50;
                            img.FixHeight = 50;

                            row.Cells.Add(id);
                            row.Cells.Add(name);
                            row.Cells.Add(author);
                            row.Cells.Add(!string.IsNullOrEmpty(createDate) ? createDate : "Null");
                            Cell cell = row.Cells.Add();
                            cell.Paragraphs.Add(img);

                        }
                        break;
                    default:
                        break;
                }

                document.Pages[1].Paragraphs.Add(table);
                document.Save(filePath);

            }
            catch (Exception e)
            {

                throw;
            }

        }

        private static string HandleCommaCsv(string data)
        {
            if (data.Contains(","))
                return "\"" + data + "\"";

            return data;
        }
    }
}
