using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class AlbumModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string Description { get; set; }
    }
}
