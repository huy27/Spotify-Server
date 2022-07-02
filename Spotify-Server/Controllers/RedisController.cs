using Application.IService;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly ICacheStrigsStack _cacheStrigsStack;
        private readonly IMusicService _musicService;

        public RedisController(ICacheStrigsStack cacheStrigsStack, IMusicService musicService)
        {
            _cacheStrigsStack = cacheStrigsStack;
            _musicService = musicService;
        }

        // GET: api/<RedisController>
        [HttpGet("GetValue")]
        public ActionResult GetValue(string key)
        {
            var result = _cacheStrigsStack.GetStrings(key);
            return Ok(result);
        }

        [HttpPost("AddValue")]
        public ActionResult AddValue(string key, string value)
        {
            _cacheStrigsStack.SetString(key, value);
            return Ok();
        }

        [HttpGet("ListMusic")]
        public async Task<ActionResult> GetListMusic()
        {
            if (_cacheStrigsStack.IsKeyExists("musics"))
            {
                var musicsCache = _cacheStrigsStack.GetList<List<SongModel>>("musics")
                                              .OrderByDescending(x => x.Id)
                                              .ToList();
                return Ok(musicsCache);
            }
            var musics = await _musicService.Get();
            var result = _cacheStrigsStack.StoreList<List<SongModel>>("musics", musics);
            if(!result)
                return BadRequest(result);

            return Ok(musics);
        }
    }
}
