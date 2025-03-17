using System;
using System.Collections.Generic;
using musly_api.Model.CMTZ;

namespace musly_api.Model
{
	public class Playlist
	{
        public string Id { get; set; }

        public string Title { get; set; }

        public IEnumerable<Track> Tracks { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}

