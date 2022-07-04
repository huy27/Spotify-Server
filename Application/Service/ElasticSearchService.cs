using Application.IService;
using Data.Entities;
using Data.Models;
using Data.Models.Elastic;
using Data.Models.Song;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionSettings connectionSettings;
        private readonly ElasticClient _elasticClient;
        private readonly SpotifyContext _context;
        private readonly string INDEX_NAME;
        private readonly int CAPACITY_MIGRATE;

        public ElasticSearchService(SpotifyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            INDEX_NAME = "musics";
            CAPACITY_MIGRATE = 200000; //số lượng data tối da có thể migrate sang ElasticSearch

            connectionSettings = new ConnectionSettings(new Uri(_configuration["ElasticSearchServer"].ToString()));
            connectionSettings.ThrowExceptions(alwaysThrow: true);
            connectionSettings.PrettyJson();
            connectionSettings.DisableDirectStreaming();
            _elasticClient = new ElasticClient(connectionSettings);
        }

        public async Task DeleteIndex(string indexName)
        {
            if (_elasticClient.Indices.Exists(indexName).Exists)
                await _elasticClient.Indices.DeleteAsync(indexName);
        }

        public async Task<bool> DeleteDocument(string indexName, int id)
        {
            var exist = await _elasticClient.Indices.ExistsAsync(indexName);
            if (!exist.Exists)
                return false; //Index không tồn tại

            var existDocument = await _elasticClient.DocumentExistsAsync<SongModel>(id, d => d
                                                .Index(indexName));
            if (!existDocument.Exists)
                return false; //Document không tồn tại

            var result = await _elasticClient.DeleteAsync<SongModel>(id, i => i.Index(indexName));
            return result.IsValid;
        }

        public async Task MigrateListToES()
        {
            if (_elasticClient.Indices.Exists(INDEX_NAME).Exists)
            {
                await _elasticClient.Indices.DeleteAsync(INDEX_NAME);
            }

            await _elasticClient.Indices.CreateAsync(INDEX_NAME,
                                index => index.Map<MusicSuggest>(
                                    x => x.AutoMap()
                                          .Properties(ps => ps
                                                .Completion(c => c
                                                    .Name(p => p.Suggest)))
                                ));
            var lstMusic = _context.Song.Select(x => new MusicSuggest
            {
                AlbumId = x.AlbumId,
                Id = x.Id,
                Author = x.Author,
                CreateDate = x.CreateDate,
                Image = x.Image,
                IsActive = x.IsActive,
                Lyric = x.Lyric,
                Name = x.Name,
                Url = x.Url,
                Suggest = new CompletionField()
                {
                    Input = new[] { x.Name, x.Author }
                }
            }).ToList();

            var length = Math.Ceiling((double)lstMusic.Count / CAPACITY_MIGRATE);

            for (int i = 0; i < length; i++)
            {
                var musics = lstMusic.Skip(i * CAPACITY_MIGRATE)
                                     .Take(CAPACITY_MIGRATE)
                                     .ToList();

                await _elasticClient.BulkAsync(b => b
                         .Index(INDEX_NAME)
                         .IndexMany(musics));
            }
        }

        public async Task<bool> AddDocument(string indexName, object document)
        {
            var exist = await _elasticClient.Indices.ExistsAsync(indexName);
            if(!exist.Exists)
                return false;

            var result = await _elasticClient.IndexAsync(document, i => i.Index(indexName));
            return result.IsValid;
        }

        public async Task<bool> UpdateDocument(int id, UpdateSongModel request)
        {
            var music = await _elasticClient.GetAsync<Song>(id, g => g.Index(INDEX_NAME));
            if (music.Source == null)
                return false;

            music.Source.Name = request.Name ?? music.Source.Name;
            music.Source.Author = request.Author ?? music.Source.Author;
            music.Source.Url = request.Url ?? music.Source.Url;
            music.Source.Image = request.Image ?? music.Source.Image;
            music.Source.Lyric = request.Lyric ?? music.Source.Lyric;
            music.Source.IsActive = request.IsActive;

            var response = await _elasticClient.UpdateAsync<Song>(id, u => u.Index(INDEX_NAME)
                                                .Doc(music.Source));
            return response.IsValid;
        }

        public async Task<List<IndexName>> GetAllIndex()
        {
            var result = await _elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All));
            var indexs = result.Indices.Keys.ToList();
            return indexs;
        }

        public async Task<List<SongModel>> GetAllDocument()
        {
            if (!_elasticClient.Indices.Exists(INDEX_NAME).Exists)
                throw new Exception($"Index {INDEX_NAME} is not exist");

            var searchResponse = await _elasticClient.SearchAsync<SongModel>(s => s
                                .Index(INDEX_NAME)
                                .MatchAll()
                                .Size(1000));
            return searchResponse.Hits.Select(x => x.Source).OrderByDescending(x => x?.Id).ToList();
        }

        public async Task<List<SongModel>> SearchByNameOrAuthor(string name, string author)
        {
            if (!_elasticClient.Indices.Exists(INDEX_NAME).Exists)
                throw new Exception($"Index {INDEX_NAME} is not exist");

            var searchResponse = await _elasticClient.SearchAsync<SongModel>(s => s
                                .Index(INDEX_NAME)
                                .Query(q => q
                                    .Match(m => m
                                        .Field(f => f.Name)
                                        .Query(name)
                                    ) || q
                                    .Match(m => m
                                        .Field(f => f.Author)
                                        .Query(author)
                                    )
                                )
                                .Size(1000));
            return searchResponse.HitsMetadata.Hits.Select(x => x.Source).OrderByDescending(x => x?.Id).ToList();
        }

        public async Task<List<SongModel>> AutoComplete(string keyword)
        {
            if (!_elasticClient.Indices.Exists(INDEX_NAME).Exists)
                throw new Exception($"Index {INDEX_NAME} is not exist");

            var searchResponse = await _elasticClient.SearchAsync<MusicSuggest>(s => s
                                .Index(INDEX_NAME)
                                .Suggest(su => su
                                          .Completion("Suggest", c => c
                                               .Field(f => f.Suggest)
                                               .Prefix(keyword)
                                               .Fuzzy(f => f
                                                   .Fuzziness(Fuzziness.Auto)
                                               )
                                               .SkipDuplicates(false)
                                               .Size(50))
                                             ));
            var suggestions = searchResponse.Suggest["Suggest"]
                                    .SelectMany(x => x
                                        .Options
                                            .Select(x => new SongModel
                                            {
                                                Id = x.Source.Id,
                                                AlbumId = x.Source.AlbumId,
                                                Name = x.Source.Name,
                                                Author = x.Source.Author,
                                                CreateDate = x.Source.CreateDate,
                                                Image = x.Source.Image,
                                                Lyric = x.Source.Lyric,
                                                Url = x.Source.Url
                                            })
                                    ).OrderByDescending(x => x?.Id).ToList();
            return suggestions;
        }

        public async Task<SongResponse> SearchByNamePaging(string name, string author, int pageIndex, int pageSize)
        {
            await _elasticClient.Indices.UpdateSettingsAsync(INDEX_NAME, s => s
                                                .IndexSettings(i => i.Setting(UpdatableIndexSettings.MaxResultWindow, 100000)));
            var query = new SearchDescriptor<SongModel>()
                                 .Query(q => q
                                    .Regexp(r => r
                                        .Field(f => f.Name).Value(name?.ToLower() + ".*"))
                                 && q
                                    .Regexp(r => r
                                        .Field(f => f.Author).Value(author?.ToLower() + ".*"))
                                 );
            var searchResponse = await _elasticClient.SearchAsync<SongModel>(s => query
                                                     .Index(INDEX_NAME)
                                                     .From(pageIndex * pageSize)
                                                     .Size(pageSize));
            var totalResponse = await _elasticClient.SearchAsync<SongModel>(s => query
                                                    .Index(INDEX_NAME)
                                                    .Scroll("30s")); // scroll timeout
            var response = new SongResponse
            {
                Songs = searchResponse.HitsMetadata?.Hits.Select(x => x?.Source)
                                                         .OrderByDescending(x => x?.Id)
                                                         .ToList(),
                Total = totalResponse.Total
            };
            return response;
        }
    }
}
