﻿using Application.IService;
using Data.Entities;
using Data.Models;
using Data.Models.Album;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class AlbumService : IAlbumService
    {
        private readonly SpotifyContext _context;

        public AlbumService(SpotifyContext context)
        {
            _context = context;
        }

        public async Task<int> Create(CreateAlbumModel request)
        {
            var album = new Album
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.Now,
                BackgroundImageUrl = request.BackgroundImageUrl,
                IsActive = true
            };
            await _context.Album.AddAsync(album);
            await _context.SaveChangesAsync();

            return album.Id;
        }

        public async Task<List<AlbumModel>> Get()
        {
            var albums = await _context.Album.Where(x => x.IsActive).Select(
                x => new AlbumModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    BackgroundImageUrl = x.BackgroundImageUrl,
                    CreatedAt = x.CreatedAt.ToString(),
                }
            ).ToListAsync();
            return albums;
        }
    }
}
