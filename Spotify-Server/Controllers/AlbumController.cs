using Application.IService;
using Data.Models.Album;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var albums = await _albumService.Get();
            return Ok(albums);
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("Create")]
        public async Task<ActionResult> Post([FromBody] CreateAlbumModel request)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var albumId = await _albumService.Create(request);
            return StatusCode(201, $"albumId: {albumId}");
        }

        [HttpPost("Update")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateAlbumModel request)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _albumService.Update(id, request);
            if (result < 1)
                return BadRequest($"Update albumId: {id} is not success");

            return Ok($"Update albumId: {id} is success");
        }

        [HttpPost("UpdateStatus")]
        public async Task<ActionResult> UpdateStatus(int id, bool isActive)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _albumService.Update(id, isActive);
            if (result < 1)
                return BadRequest($"Update status albumId: {id} is not success");

            return Ok($"Update status albumId: {id} to {isActive} is success");
        }
    }
}
