using Data.Models;
using Data.Models.Song;
using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IElasticSearchService
    {
        Task MigrateListToES();
        Task DeleteIndex(string indexName);
        Task<bool> UpdateDocument(int id, UpdateSongModel request);
        Task<List<IndexName>> GetAllIndex();
        Task<List<SongModel>> GetAllDocument();
        Task<List<SongModel>> SearchByNameOrAuthor(string name, string author);
        Task<List<SongModel>> AutoComplete(string keyword);
    }
}
