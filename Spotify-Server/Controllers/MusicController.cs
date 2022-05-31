using Application.IService;
using Data.Models.Song;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        // GET: api/<MusicController>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var songs = await _musicService.Get();
            return Ok(songs);
        }

        // GET api/<MusicController>/5
        [HttpGet("{albumId}")]
        public async Task<ActionResult> Get(int albumId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songs = await _musicService.GetByAlbumId(albumId);
            return Ok(songs);
        }

        // POST api/<MusicController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateSongModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var songId = await _musicService.Create(request);
            return StatusCode(201, $"songId: {songId} in albumId: {request.AlbumId}");
        }

        // PUT api/<MusicController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MusicController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
