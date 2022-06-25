using Application.IService;
using Data.Models;
using Data.Models.Song;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElasticSearchController : ControllerBase
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ElasticSearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpPost("MigrateDataToES")]
        public async Task<ActionResult> MigrateDataToES()
        {
            await _elasticSearchService.MigrateListToES();
            return Ok("Migrate Data is success");
        }

        [HttpDelete("DeleteIndex")]
        public async Task<ActionResult> DeleteIndex(string indexName)
        {
            await _elasticSearchService.DeleteIndex(indexName);
            return Ok();
        }

        [HttpPut("UpdateDocument")]
        public async Task<ActionResult> UpdateDocument(int id, UpdateSongModel request)
        {
            var result = await _elasticSearchService.UpdateDocument(id, request);
            if(result)
                return Ok("Update success");
            return BadRequest();
        }

        [HttpGet("GetAllIndex")]
        public async Task<ActionResult> GetAllIndex()
        {
            var indexs = await _elasticSearchService.GetAllIndex();
            return Ok(indexs);
        }

        [HttpGet("GetAllDocument")]
        public async Task<ActionResult> GetAllDocument()
        {
            var result = await _elasticSearchService.GetAllDocument();
            return Ok(result);
        }

        [HttpGet("SearchByNameOrAuthor")]
        public async Task<ActionResult> SearchByNameOrAuthor(string name, string author)
        {
            var result = await _elasticSearchService.SearchByNameOrAuthor(name, author);
            return Ok(result);
        }

        [HttpGet("AutoComplete")]
        public async Task<ActionResult> AutoComplete(string keywork)
        {
            if(string.IsNullOrEmpty(keywork))
                return Ok();
            var result = await _elasticSearchService.AutoComplete(keywork);
            return Ok(result);
        }
    }
}
