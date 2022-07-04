using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.Elastic
{
    public class SongResponse
    {
        public List<SongModel> Songs { get; set; }
        public long Total { get; set; }
    }
}
