using Application.IService;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Application.Ultilities
{
    public class DatabaseHangfireService : IDatabaseHangfireService
    {
        private readonly IConfiguration _configuration;

        public DatabaseHangfireService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Truncate()
        {
            string sqlConnectionString = _configuration.GetConnectionString("SpotifyConnection");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"Scripts\ClearDataHangfire.sql");

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
