using Application.IService;
using Application.Ultilities;
using Data.Models;
using Data.Models.Song;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ElasticSearchController : ControllerBase
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ElasticSearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        #region MigrateDataToES
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("MigrateDataToES")]
        public async Task<ActionResult> MigrateDataToES()
        {
            await _elasticSearchService.MigrateListToES();
            return Ok("Migrate Data is success");
        }
        #endregion

        #region AddDocument
        [Authorize(Roles = UserRoles.Admin)]
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
        [Authorize(Roles = UserRoles.Admin)]
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
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("DeleteIndex")]
        public async Task<ActionResult> DeleteIndex(string indexName)
        {
            await _elasticSearchService.DeleteIndex(indexName);
            return Ok();
        }
        #endregion

        #region UpdateDocument
        [Authorize(Roles = UserRoles.Admin)]
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
        [AllowAnonymous]
        [HttpGet("GetAllIndex")]
        public async Task<ActionResult> GetAllIndex()
        {
            var indexs = await _elasticSearchService.GetAllIndex();
            return Ok(indexs);
        }
        #endregion

        #region GetAllDocument
        [AllowAnonymous]
        [HttpGet("GetAllDocument")]
        public async Task<ActionResult> GetAllDocument()
        {
            var result = await _elasticSearchService.GetAllDocument();
            return Ok(result);
        }
        #endregion

        #region SearchByNameOrAuthor
        [AllowAnonymous]
        [HttpGet("SearchByNameOrAuthor")]
        public async Task<ActionResult> SearchByNameOrAuthor(string name, string author)
        {
            var result = await _elasticSearchService.SearchByNameOrAuthor(name, author);
            return Ok(result);
        }
        #endregion

        #region AutoComplete
        [AllowAnonymous]
        [HttpGet("AutoComplete")]
        public async Task<ActionResult> AutoComplete(string keywork)
        {
            if(string.IsNullOrEmpty(keywork))
                return Ok();
            var result = await _elasticSearchService.AutoComplete(keywork);
            return Ok(result);
        }
        #endregion

        #region SearchByNamePaging
        [AllowAnonymous]
        [HttpGet("SearchByNamePaging")]
        public async Task<ActionResult> SearchByNamePaging(string name, string author, int pageIndex = 0, int pageSize = 10)
        {
            var result = await _elasticSearchService.SearchByNamePaging(name, author, pageIndex, pageSize);
            return Ok(result);
        }
        #endregion
    }
}
