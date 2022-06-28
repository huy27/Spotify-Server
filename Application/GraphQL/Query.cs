using Application.IService;
using Data.Entities;
using Data.Models;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.GraphQL
{
    public class Query
    {
        private readonly IMusicService _musicService;
        private readonly SpotifyContext _context;

        public Query(IMusicService musicService, SpotifyContext context)
        {
            _musicService = musicService;
            _context = context;
        }

        [GraphQLDescription("Get all song")]
        public async Task<List<Song>> GetSongs()
        {
            return await _context.Song.ToListAsync();
        }

        [GraphQLDescription("Get song by Id")]
        public async Task<Song> GetSong(int id)
        {
            return await _context.Song.FirstOrDefaultAsync(x => x.Id == id);
        }

        [GraphQLDescription("Get song by Name")]
        public async Task<List<Song>> GetSongsByName(string name)
        {
            return await _context.Song.Where(x => x.Name.Contains(name)).ToListAsync();
        }

        [GraphQLDescription("Get all album")]
        public async Task<List<Album>> GetAlbums()
        {
            return await _context.Album.ToListAsync();
        }
    }
}
