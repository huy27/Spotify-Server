using Data.Models;
using Data.Models.Album;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IAlbumService
    {
        Task<List<AlbumModel>> Get();
        Task<int> Create(CreateAlbumModel request);
        Task<int> Update(int id, UpdateAlbumModel request);
        Task<int> Update(int id, bool isActive);
    }
}
