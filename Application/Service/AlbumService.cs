using Application.IService;
using Application.Ultilities;
using Data.Entities;
using Data.Models;
using Data.Models.Album;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class AlbumService : IAlbumService
    {
        private readonly SpotifyContext _context;
        private readonly IQueueService _queueService;

        public AlbumService(SpotifyContext context, IQueueService queueService)
        {
            _context = context;
            _queueService = queueService;
        }

        public async Task<int> Create(CreateAlbumModel request)
        {
            var album = new Album
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.Now,
                BackgroundImageUrl = request.BackgroundImageUrl,
                IsActive = true
            };
            await _context.Album.AddAsync(album);
            await _context.SaveChangesAsync();
            await _queueService.SenderAsync(JsonConvert.SerializeObject(album));
            return album.Id;
        }

        public async Task<List<AlbumModel>> Get()
        {
            var albums = await _context.Album.Where(x => x.IsActive).Select(
                x => new AlbumModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    BackgroundImageUrl = x.BackgroundImageUrl,
                    CreatedAt = TimeUltility.CalculateRelativeTime(x.CreatedAt),
                }
            ).ToListAsync();
            return albums;
        }

        public async Task<int> Update(int id, UpdateAlbumModel request)
        {
            var album = await _context.Album.FirstOrDefaultAsync(x => x.Id == id);
            if (album == null)
                return -1;

            album.Name = request.Name;
            album.Description = request.Description;
            album.BackgroundImageUrl = request.BackgroundImageUrl;

            _context.Album.Update(album);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(int id, bool isActive)
        {
            var album = await _context.Album.FirstOrDefaultAsync(x => x.Id == id);
            if (album == null)
                return -1;

            album.IsActive = isActive;

            _context.Album.Update(album);
            return await _context.SaveChangesAsync();
        }
    }
}
