using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using musly_api.Model;
using musly_api.Model.CMTZ;
using musly_api.Repository;
using musly_api.Services;

namespace musly_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController: ControllerBase
	{
        private RedisPlaylistRepository _playlistRepo { get; set; }

        public SearchService _searchService { get; set; }

        public IConfiguration _config { get; set; }


        public PlaylistController(RedisPlaylistRepository playlistRepo, SearchService searchService, IConfiguration config)
		{
            _playlistRepo = playlistRepo;
            _searchService = searchService;
            _config = config;

        }

        [HttpGet]
        public async Task<IEnumerable<Playlist>> GetPlaylist(int? count)
        {
            if (count == null)
            {
                count = 25;
            }

            var playlists = await _playlistRepo.GetAllTracks();

            return playlists.OrderByDescending(p=> p.CreatedOn).Take(count.Value);
        }


        [HttpGet("{playlistId}")]
        public Task<Playlist> GetPlaylistById(string playlistId)
        {
            return _playlistRepo.GetPlaylistById(playlistId);
        }


        [HttpPost]
        public async Task<Playlist> CreatePlaylist([FromBody] PlaylistPost request)
        {
            var results = new ConcurrentBag<IEnumerable<Track>>();
            var tracks = new List<Track>();
            var playlist = new Playlist();


            Parallel.ForEach(request.songs, q =>
            {
                var searchResults = _searchService.SearchTracks(q, 1, 1);
                var items = searchResults.Items;
                results.Add(items);
            });

            var resultList = results.ToList();
            resultList.ForEach(l => {
                tracks.AddRange(l);
            });

            playlist.Id = Guid.NewGuid().ToString();
            playlist.Tracks = tracks.Select(GetTrack);
            playlist.Title = request.title;
            playlist.CreatedOn = DateTime.Now;
            playlist.PlaylistUrl = @"https://www.certifiedmixtapez.com/ai-playlists/" + playlist.Id;
            await _playlistRepo.SaveTrack(playlist);

            return playlist;
        }
        

        private Track GetTrack(Track track)
        {
            try
            {
                String MixtapesStorage = _config.GetValue<string>("musly:MixtapesStorage");
                String AlbumsStorage = _config.GetValue<string>("musly:AlbumsStorage");

                var path = track.Album.IsMixtape.HasValue ? track.Album.IsMixtape.Value ? MixtapesStorage : AlbumsStorage : MixtapesStorage;
                var filePath = String.Format("{0}/{1}/{2}", path, track.Album.FilePath, track.TrackTitle);
                var coverFile = String.Format("{0}/{1}/{2}", path, track.Album.FilePath, track.Album.CoverImageName);

                return new Track()
                {
                    Id = track.Id,
                    TrackTitle = track.TrackTitle,
                    CoverImageName = coverFile,
                    TotalViews = track.Album.TotalViews,
                    TotalDownloads = track.Album.TotalDownloads,
                    TotalVotes = track.Album.TotalVotes,
                    Upvotes = track.Album.Upvotes,
                    TrackURL = filePath + track.Extension,
                    AlbumId = track.AlbumId,
                    Extension = track.Extension,
                    Size = track.Size,
                    Order = track.Order,
                    CreatedOn = track.CreatedOn,
                    ModifiedOn = track.ModifiedOn,
                    Album = GetAlbum(track.Album),
                };
            }
            catch (Exception ex) { }
            return track;
        }

        private Album GetAlbum(Album album)
        {
            try
            {
                String MixtapesStorage = _config.GetValue<string>("musly:MixtapesStorage");
                String AlbumsStorage = _config.GetValue<string>("musly:AlbumsStorage");

                var path = album.IsMixtape.HasValue ? album.IsMixtape.Value ? MixtapesStorage : AlbumsStorage : MixtapesStorage;
                var filePath = String.Format("{0}/{1}/{2}", path, album.FilePath, album.CoverImageName);
                var thumbfilePath = String.Format("{0}/{1}/tn_{2}", path, album.FilePath, album.CoverImageName);


                return new Album()
                {
                    Title = album.Title,
                    Id = album.Id,
                    CoverImageName = filePath,
                    FilePath = album.FilePath,
                    ThumbImg = thumbfilePath,
                    AlbumStatus = album.AlbumStatus,
                    SigningType = album.SigningType,
                    AlbumType = album.AlbumType,
                    Artists = album.Artists,
                    DjUser = album.DjId.HasValue ? album.DjUser : null,
                    KeyWords = album.KeyWords,
                    GenreType = album.GenreType,
                    Description = album.Description,
                    RefId = album.RefId,
                    TotalDownloads = album.TotalDownloads,
                    TotalViews = album.TotalViews,
                    Upvotes = album.Upvotes,
                    Downvotes = album.Downvotes,
                    TotalVotes = album.TotalVotes,
                    UploadedByUser = null,
                    CreatedOn = album.CreatedOn,
                    ModifiedOn = album.ModifiedOn,
                    AffiliateLink = album.AffiliateLink,
                    InstagramLink = album.InstagramLink,
                    IsMixtape = album.IsMixtape,
                    RecordLabel = album.RecordLabel,
                    TwitterLink = album.TwitterLink,
                    ItunesAlbumType = album.ItunesAlbumType,
                };
            }
            catch (NullReferenceException ex)
            {
                return album;
            }
        }
    }

    public class PlaylistPost
    {
        public string title { get; set; }
        public string description { get; set; }
        public List<string> songs { get; set; }
    }
}

