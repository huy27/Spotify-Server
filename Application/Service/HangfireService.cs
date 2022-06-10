using Application.IService;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Application.Ultilities
{
    public class HangfireService : IHangfireService
    {
        private readonly IConfiguration _configuration;

        public HangfireService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DeleteOldFile()
        {
            var _userContentFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\user-content");
            var files = Directory.GetFiles(_userContentFolder);

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    //Remove all file was created before 7 days
                    if (fileInfo.CreationTime < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(-7),
                                                    TimeZoneInfo.FindSystemTimeZoneById(_configuration["Timezone"])))
                    {
                        fileInfo.Delete();
                    }
                }
            }
        }

        public async Task TruncateDB()
        {
            string sqlConnectionString = _configuration.GetConnectionString("SpotifyConnection");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Scripts\ClearDataHangfire.sql");

            string script = await File.ReadAllTextAsync(filePath);

            using (SqlConnection conn = new SqlConnection(sqlConnectionString))
            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
        }
    }
}
