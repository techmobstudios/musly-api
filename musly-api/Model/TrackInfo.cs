using System;
using System.Collections.Generic;

namespace musly_api.Model
{
	public class TrackInfo
	{
		public TrackInfo()
		{
		}

        public string Id { get; set; }
        public string Title { get; set; }
        public string AlbumTitle { get; set; }
        public string Artist { get; set; }
        public string Description { get; set; }
        public string GenreCode { get; set; }
        public string Keywords { get; set; }
        public string DJ { get; set; }
        public string Url { get; set; }
        public float[] AudioFeatures { get; set; }
        public Dictionary<string, double> Similarities { get; set; }

    }
}

