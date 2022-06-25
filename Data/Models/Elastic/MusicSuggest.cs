using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class MusicSuggest
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

        public CompletionField Suggest { get; set; }
    }
}
