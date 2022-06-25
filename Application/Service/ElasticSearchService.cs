﻿using Application.IService;
using Data.Entities;
using Data.Models;
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
        private readonly ElasticClient _elasticClient;
        private readonly SpotifyContext _context;
        private readonly string INDEX_NAME;
        private readonly int CAPACITY_MIGRATE;

        public ElasticSearchService(SpotifyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            INDEX_NAME = "musics";
            CAPACITY_MIGRATE = 200000;

            var connectionSettings = new ConnectionSettings(new Uri(_configuration["ElasticSearchServer"].ToString()));
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
            var count = _context.Song.Count();
            var length = Math.Ceiling((double)count / CAPACITY_MIGRATE);

            for (int i = 0; i < length; i++)
            {
                var musics = await _context.Song.Select(x => new MusicSuggest
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
                }).Skip(i * CAPACITY_MIGRATE).Take(CAPACITY_MIGRATE).ToListAsync();

                await _elasticClient.BulkAsync(b => b
                         .Index(INDEX_NAME)
                         .IndexMany(musics));
            }
        }

        public async Task<bool> UpdateDocument(int id, UpdateSongModel request)
        {
            var music = await _elasticClient.GetAsync<Song>(id, g => g.Index(INDEX_NAME));
            if (music.Source == null)
                return false;

            music.Source.Name = request.Name;
            music.Source.Author = request.Author;
            music.Source.Url = request.Url;
            music.Source.Image = request.Image;
            music.Source.Lyric = request.Lyric;
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
    }
}
