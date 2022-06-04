using Application.IService;
using Application.Ultilities;
using Data.Entities;
using Data.Models;
using Microsoft.EntityFrameworkCore;
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

        public BackupDataService(SpotifyContext context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
        }

        public async Task Backup()
        {
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
            }).OrderByDescending(x => x.CreateDate).ToListAsync();

            var albums = await _context.Album.Select(x => new AlbumModel
            {
                Id = x.Id,
                Name = x.Name,
                BackgroundImageUrl = x.BackgroundImageUrl,
                CreatedAt = x.CreatedAt.ToString(),
                Description = x.Description
            }).OrderByDescending(x => x.CreatedAt).ToListAsync();

            await FileService.SaveFile(JsonConvert.SerializeObject(songs, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), "Music.json");

            await FileService.SaveFile(JsonConvert.SerializeObject(albums, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), "Album.json");

            _mailService.SendMail("huy27297@gmail.com", "Backup data success");
        }
    }
}
