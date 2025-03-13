using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using CMTZ.Common;
//using CMTZ.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using musly_api.Model;
using musly_api.Model.CMTZ;
using musly_api.Services;
using Nest;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Serialization;

namespace musly_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SongsController : ControllerBase
    {

        public MuslyService _muslyService { get; set; }

        public TrackExport _trackEport { get; set; }

        public SearchService _searchService { get; set; }
        //AppSettings Configuration { get; set; }


        public SongsController(ILogger<SongsController> logger, MuslyService muslyService, SearchService searchService /*, TrackExport trackExport , IOptions<AppSettings> appsettings*/)
        {
            _muslyService = muslyService;
            _searchService = searchService;
            //_trackEport = trackExport;
            //Configuration = appsettings.Value;

        }

        [HttpGet]
        public IEnumerable<String> Get(string query= "bigx")
        {
            return _muslyService.cf.trackFiles.AsEnumerable().Where(x => x.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet("search")]
        public DSearchResult<Track> Search(string query = "bigx")
        {
            return _searchService.SearchTracks(query, 1, 10);
        }

        //[HttpPost]
        //public Task<IEnumerable<TrackInfo>> Search(string[] songs)
        //{
        //return _muslyService.cf.trackFiles.AsEnumerable().Where(x => x.Contains(query, StringComparison.OrdinalIgnoreCase));
        //}

        /*[HttpGet("export")]
        public async Task<IEnumerable<TrackInfo>> Export()
        {
            //var result = new musly_api.Model.Result { };
            var tracks = await _trackEport.Run();
            //var trackList = tracks.Where(t => t.Album.GenreTypeCode == 2);
            //result.ResponseObject = trackList;
            //var error = JObject.FromObject(result, pascalCaseSerializer);
            //var eObj = new PResult { obj = error };
            return tracks;
         
        }

        [HttpGet("process")]
        public async Task<IEnumerable<TrackInfo>> Process()
        {
            return await _trackEport.Process();
            //return tracks.Select(GetTrack);
        }

        private Track GetTrack(Track track)
        {
            try
            {
                var path = track.Album.IsMixtape.HasValue ? track.Album.IsMixtape.Value ? Configuration.MixtapesStorage : Configuration.AlbumsStorage : Configuration.MixtapesStorage;
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
                var path = album.IsMixtape.HasValue ? album.IsMixtape.Value ? Configuration.MixtapesStorage : Configuration.AlbumsStorage : Configuration.MixtapesStorage;
                var filePath = String.Format("{0}/{1}/{2}", path, album.FilePath, album.CoverImageName);
                var thumbfilePath = String.Format("{0}/{1}/tn_{2}", path, album.FilePath, album.CoverImageName);

                //return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.(Path)).ToString();


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
        }*/
    }
}
