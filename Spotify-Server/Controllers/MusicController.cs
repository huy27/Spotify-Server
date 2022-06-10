using Application.IService;
using Application.Ultilities;
using Data.Models.Song;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;
        private readonly IConfiguration _configuration;

        public MusicController(IMusicService musicService, IConfiguration configuration)
        {
            _musicService = musicService;
            _configuration = configuration;
        }

        #region Get
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var songs = await _musicService.Get();
            return Ok(songs);
        }
        #endregion

        #region GetByAlbumId
        [HttpGet("{albumId}")]
        public async Task<ActionResult> Get(int albumId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByAlbumId(albumId);
            return Ok(songs);
        }
        #endregion

        #region SearchByCondition
        [HttpGet("SearchByCondition")]
        public async Task<ActionResult> SearchByCondition(string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByCondition(name);
            return Ok(songs);
        }
        #endregion

        #region ExportCSV
        [HttpGet("ExportCSV")]
        public ActionResult ExportCSV()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dateBackup = TimeZoneInfo.ConvertTimeFromUtc(
                                            DateTime.UtcNow,
                                            TimeZoneInfo.FindSystemTimeZoneById(_configuration["Timezone"])
                                            ).ToString("dd-MM-yyyy");
            var filePath = FileService.GetUrl($"Music-{dateBackup}.csv");
            if (string.IsNullOrEmpty(filePath))
                return NoContent();

            return Ok(filePath);
        }
        #endregion

        #region Create
        [HttpPost("Create")]
        public async Task<ActionResult> Create(int albumId, [FromBody] CreateSongModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _musicService.Create(albumId, request);
            if (result < 0)
                return BadRequest($"AlbumId {albumId} is not exists");

            return StatusCode(201, $"songId: {result} in albumId: {albumId}");
        }
        #endregion

        #region UpdateStatus
        [HttpPost("UpdateStatus")]
        public async Task<ActionResult> UpdateStatus(int id, bool isActive)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _musicService.Update(id, isActive);

            if (result < 1)
                return BadRequest($"Update id: {id} is not success");
            return Ok($"Status id: {id} to {isActive}");
        }
        #endregion

        #region Update
        [HttpPost("Update")]
        public async Task<ActionResult> Update(int id, int albumId, UpdateSongModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _musicService.Update(id, albumId, request);
            if (result < 1)
                return BadRequest($"Update id: {id} is not success");
            return Ok($"Status id: {id} to success");
        }
        #endregion

    }
}
