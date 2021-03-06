using Application.IService;
using Application.Ultilities;
using Data.Enums;
using Data.Models.Song;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;
        private readonly IBackupDataService _backupDataService;
        private readonly IConfiguration _configuration;

        public MusicController(IMusicService musicService, IBackupDataService backupDataService, IConfiguration configuration)
        {
            _musicService = musicService;
            _backupDataService = backupDataService;
            _configuration = configuration;
        }

        #region Get
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var songs = await _musicService.Get();
            return Ok(songs);
        }
        #endregion

        #region GetByAlbumId
        [AllowAnonymous]
        [HttpGet("{albumId}")]
        public async Task<ActionResult> Get(int albumId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByAlbumId(albumId);
            return Ok(songs);
        }
        #endregion

        #region GetByName
        [AllowAnonymous]
        [HttpGet("GetByName/{name}")]
        public async Task<ActionResult> GetByName(string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByName(name);
            return Ok(songs);
        }
        #endregion

        #region SearchByCondition
        [AllowAnonymous]
        [HttpGet("SearchByCondition")]
        public async Task<ActionResult> SearchByCondition(string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByCondition(name);
            return Ok(songs);
        }
        #endregion

        #region ExportFile
        [HttpPost("ExportFile")]
        public async Task<ActionResult> ExportFile(string request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TypeExportFile typeExportFile;
            if (!Enum.TryParse<TypeExportFile>(request, out typeExportFile))
                return BadRequest($"System don't support export type: {request}");

            var filePath = GetFilePath(typeExportFile);

            if (string.IsNullOrEmpty(filePath))
            {
                await _backupDataService.Backup(false);
                filePath = GetFilePath(typeExportFile);
            }

            var fileData = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileData, "application/force-download", $"Music.{typeExportFile}");
        }
        #endregion

        #region Create
        [Authorize(Roles = UserRoles.Admin)]
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
        [Authorize(Roles = UserRoles.Admin)]
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
        [Authorize(Roles = UserRoles.Admin)]
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

        #region ImportCsv
        [Authorize(Roles = UserRoles.Admin)]
        [Consumes("multipart/form-data")]
        [HttpPost("ImportCsv")]
        public async Task<ActionResult> ImportCsv(IFormFile file)
        {
            await _musicService.ImportCsv(file);
            return Ok();
        }
        #endregion

        private string GetFilePath(TypeExportFile typeExportFile)
        {
            var filePath = "";
            var dateBackup = TimeZoneInfo.ConvertTimeFromUtc(
                                            DateTime.UtcNow,
                                            TimeZoneInfo.FindSystemTimeZoneById(_configuration["Timezone"])
                                            ).ToString("dd-MM-yyyy");
            switch (typeExportFile)
            {
                case TypeExportFile.json:
                    filePath = FileService.GetUrl($"Music-{dateBackup}.json");
                    break;
                case TypeExportFile.csv:
                    filePath = FileService.GetUrl($"Music-{dateBackup}.csv");
                    break;
                case TypeExportFile.pdf:
                    filePath = FileService.GetUrl($"Music-{dateBackup}.pdf");
                    break;
                default:
                    break;
            }
            return filePath;
        }
    }
}
