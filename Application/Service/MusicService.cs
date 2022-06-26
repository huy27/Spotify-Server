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

        public async Task<int> Create(int albumId, CreateSongModel request)
        {
            var album = await _context.Album.FirstOrDefaultAsync(x => x.Id == albumId && x.IsActive);
            if (album == null)
                return -1;

            var song = new Song
            {
                AlbumId = albumId,
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

        public async Task<List<SongModel>> GetByCondition(string name)
        {
            var songs = await _context.Song.Where(x => x.Name.Contains(name) 
                                                || x.Author.Contains(name) 
                                                && x.IsActive 
                                                && x.Album.IsActive)
                .Select(x => new SongModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Image,
                    Author = x.Author,
                    Lyric = x.Lyric,
                    Url = x.Url,
                    AlbumId = x.AlbumId,
                    CreateDate = x.CreateDate
                }).OrderBy(x => x.CreateDate).ToListAsync();
            return songs;
        }

        public async Task<List<SongModel>> Get()
        {
            var songs = await _context.Song.Where(x => x.IsActive && x.Album.IsActive)
                .Select(x => new SongModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Image,
                    Author = x.Author,
                    Lyric = x.Lyric,
                    Url = x.Url,
                    AlbumId = x.AlbumId,
                    CreateDate = x.CreateDate
                }).OrderBy(x => x.CreateDate).ToListAsync();
            return songs;
        }

        public async Task<List<SongModel>> GetByAlbumId(int albumId)
        {
            var songs = await _context.Song.Where(x => x.AlbumId == albumId && x.IsActive && x.Album.IsActive)
                .Select(x => new SongModel
                {
                    AlbumId = x.AlbumId,
                    Id = x.Id,
                    Name = x.Name,
                    Author = x.Author,
                    CreateDate = x.CreateDate,
                    Image = x.Image,
                    Lyric = x.Lyric,
                    Url = x.Url,
                }).ToListAsync();
            return songs;
        }

        public async Task<List<SongModel>> GetByName(string name)
        {
            return await _context.Song.Where(x => x.Name == name)
                        .Select(x => new SongModel
                        {
                            AlbumId = x.AlbumId,
                            Id = x.Id,
                            Name = x.Name,
                            Author = x.Author,
                            CreateDate = x.CreateDate,
                            Image = x.Image,
                            Lyric = x.Lyric,
                            Url = x.Url,
                        }).ToListAsync();
        }

        public async Task<int> Update(int id, int albumId, UpdateSongModel request)
        {
            var song = await _context.Song.FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.Album.IsActive);
            if (song == null)
                return -1;

            song.AlbumId = albumId;
            song.Name = request.Name;
            song.Author = request.Author;
            song.Url = request.Url;
            song.Image = request.Image;
            song.Lyric = request.Lyric;
            song.IsActive = request.IsActive;

            _context.Song.Update(song);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(int id, bool isActive)
        {
            var song = await _context.Song.FirstOrDefaultAsync(x => x.Id == id && x.Album.IsActive);
            if (song == null)
                return -1;

            song.IsActive = isActive;
            _context.Song.Update(song);
            var result = await _context.SaveChangesAsync();
            return result;
        }
    }
}
