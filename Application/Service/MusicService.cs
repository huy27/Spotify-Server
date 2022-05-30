using Application.IService;
using Data.Entities;
using Data.Models;
using Data.Models.Song;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class MusicService : IMusicService
    {
        private readonly SpotifyContext _context;

        public MusicService(SpotifyContext context)
        {
            _context = context;
        }

        public async Task<int> Create(CreateSongModel request)
        {
            var song = new Song
            {
                AlbumId = request.AlbumId,
                Name = request.Name,
                Author = request.Author,
                Image = request.Image,
                Lyric = request.Lyric,
                Url = request.Url,
                CreateDate = DateTime.Now,
                IsActive = true
            };
            await _context.Song.AddAsync(song);
            await _context.SaveChangesAsync();

            return song.Id;
        }

        public async Task<List<SongModel>> Get()
        {
            var songs = await _context.Song.Where(x => x.IsActive).Select(x => new SongModel
            {
                Id = x.Id,
                Name = x.Name,
                Image = x.Image,
                Author = x.Author,
                Lyric = x.Lyric,
                Url = x.Url,
                AlbumId = x.AlbumId,
                CreateDate = x.CreateDate
            }).ToListAsync();
            return songs;
        }
    }
}
