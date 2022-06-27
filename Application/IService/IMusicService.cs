using Data.Models;
using Data.Models.Song;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IMusicService
    {
        Task<List<SongModel>> Get();
        Task<List<SongModel>> GetByAlbumId(int albumId);
        Task<List<SongModel>> GetByCondition(string name);
        Task<List<SongModel>> GetByName(string name);
        Task<int> Create(int albumId, CreateSongModel request);
        Task<int> Update(int id, bool isActive);
        Task<int> Update(int id, int albumId, UpdateSongModel request);

    }
}
