using Application.IService;
using Application.Ultilities;
using Data.Entities;
using Data.Enums;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class BackupDataService : IBackupDataService
    {
        private readonly SpotifyContext _context;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;

        public BackupDataService(SpotifyContext context, IMailService mailService, IConfiguration configuration)
        {
            _context = context;
            _mailService = mailService;
            _configuration = configuration;
        }

        public async Task Backup()
        {
            var dateBackup = TimeZoneInfo.ConvertTimeFromUtc(
                                            DateTime.UtcNow,
                                            TimeZoneInfo.FindSystemTimeZoneById(_configuration["Timezone"])
                                            ).ToString("dd-MM-yyyy");

            var songs = await _context.Song.Select(x => new SongModel
            {
                Id = x.Id,
                Name = x.Name,
                Image = x.Image,
                Author = x.Author,
                Lyric = x.Lyric,
                Url = x.Url,
                AlbumId = x.AlbumId,
                CreateDate = x.CreateDate
            }).OrderByDescending(x => x.Id).ToListAsync();

            var albums = await _context.Album.Select(x => new AlbumModel
            {
                Id = x.Id,
                Name = x.Name,
                BackgroundImageUrl = x.BackgroundImageUrl,
                CreatedAt = x.CreatedAt.ToString(),
                Description = x.Description
            }).OrderByDescending(x => x.Id).ToListAsync();

            await FileService.SaveCsvFile<AlbumModel>(albums, $"Album-{dateBackup}.csv", TypeCsvFile.Album);
            await FileService.SaveCsvFile<SongModel>(songs, $"Music-{dateBackup}.csv", TypeCsvFile.Music);

            await FileService.SaveFile(JsonConvert.SerializeObject(songs, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), $"Music-{dateBackup}.json");

            await FileService.SaveFile(JsonConvert.SerializeObject(albums, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), $"Album-{dateBackup}.json");

            var filePaths = new List<string>
            {
                FileService.GetUrl($"Music-{dateBackup}.json"),
                FileService.GetUrl($"Album-{dateBackup}.json"),
                FileService.GetUrl($"Music-{dateBackup}.csv"),
                FileService.GetUrl($"Album-{dateBackup}.csv")
            };
            _mailService.SendMail("huy27297@gmail.com",
                $"Backup data of date : {dateBackup} is success",
                "Notification Backup Data",
                filePaths);
        }
    }
}
