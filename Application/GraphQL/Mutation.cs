using Application.IService;
using Data.Entities;
using Data.Models.Album;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Application.GraphQL
{
    public class Mutation
    {
        private readonly IMusicService _musicService;
        private readonly IAlbumService _albumService;
        private readonly SpotifyContext _context;

        public Mutation(IMusicService musicService, IAlbumService albumService, SpotifyContext context)
        {
            _musicService = musicService;
            _albumService = albumService;
            _context = context;
        }

        [GraphQLDescription("Add new album")]
        public async Task<int> AddAlbum(CreateAlbumModel createAlbumModel)
        {
            var result = await _albumService.Create(createAlbumModel);
            return result;
        }

        [GraphQLDescription("Delete album")]
        public async Task<int> DeleteAlbum(int id)
        {
            var album = await _context.Album.FirstOrDefaultAsync(x => x.Id == id);

            if (album == null) return -1;

            var result = _context.Album.Remove(album);
            return await _context.SaveChangesAsync();
        }
    }
}
