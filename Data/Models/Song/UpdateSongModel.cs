using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.Song
{
    public class UpdateSongModel
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Lyric { get; set; }
        public bool IsActive { get; set; }
    }
}
