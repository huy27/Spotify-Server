using Application.IService;
using Data.Models;
using Data.Models.Song;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        #region MigrateDataToES
        [HttpPost("MigrateDataToES")]
        public async Task<ActionResult> MigrateDataToES()
        {
            await _elasticSearchService.MigrateListToES();
            return Ok("Migrate Data is success");
        }
        #endregion

        #region AddDocument
        [HttpPost("AddDocument")]
        public async Task<ActionResult> AddDocument(string indexName, SongModel request)
        {
            var result = await _elasticSearchService.AddDocument(indexName, request);
            if (result)
                return Ok($"Add document to index: {indexName} is success");
            return BadRequest(result);
        }
        #endregion

        #region DeleteDocument
        [HttpDelete("DeleteDocument")]
        public async Task<ActionResult> DeleteDocument(string indexName, int id)
        {
            var result = await _elasticSearchService.DeleteDocument(indexName, id);
            if (result)
                return Ok($"Delete document from index: {indexName} is success");
            return BadRequest(result);
        }
        #endregion

        #region DeleteIndex
        [HttpDelete("DeleteIndex")]
        public async Task<ActionResult> DeleteIndex(string indexName)
        {
            await _elasticSearchService.DeleteIndex(indexName);
            return Ok();
        }
        #endregion

        #region UpdateDocument
        [HttpPut("UpdateDocument")]
        public async Task<ActionResult> UpdateDocument(int id, UpdateSongModel request)
        {
            var result = await _elasticSearchService.UpdateDocument(id, request);
            if(result)
                return Ok("Update success");
            return BadRequest();
        }
        #endregion

        #region GetAllIndex
        [HttpGet("GetAllIndex")]
        public async Task<ActionResult> GetAllIndex()
        {
            var indexs = await _elasticSearchService.GetAllIndex();
            return Ok(indexs);
        }
        #endregion

        #region GetAllDocument
        [HttpGet("GetAllDocument")]
        public async Task<ActionResult> GetAllDocument()
        {
            var result = await _elasticSearchService.GetAllDocument();
            return Ok(result);
        }
        #endregion

        #region SearchByNameOrAuthor
        [HttpGet("SearchByNameOrAuthor")]
        public async Task<ActionResult> SearchByNameOrAuthor(string name, string author)
        {
            var result = await _elasticSearchService.SearchByNameOrAuthor(name, author);
            return Ok(result);
        }
        #endregion

        #region AutoComplete
        [HttpGet("AutoComplete")]
        public async Task<ActionResult> AutoComplete(string keywork)
        {
            if(string.IsNullOrEmpty(keywork))
                return Ok();
            var result = await _elasticSearchService.AutoComplete(keywork);
            return Ok(result);
        }
        #endregion
    }
}
