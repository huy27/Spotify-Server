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
        Task<int> Create(CreateSongModel request);
    }
}
