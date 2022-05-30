using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Data.Entities
{
    public partial class Song
    {
        public int AlbumId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public string Lyric { get; set; }
        public DateTime? CreateDate { get; set; }

        public virtual Album Album { get; set; }
    }
}
