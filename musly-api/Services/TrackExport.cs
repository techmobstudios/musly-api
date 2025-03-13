using System;
using System.Collections.Generic;
//using CMTZ.Services;
//using CMTZ.Entities;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Nest;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using musly_api.Model;
//using CMTZ.Common;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace musly_api.Services
{
	public class TrackExport
	{
       /* public DTrackService DTrackService { get; }
        IHostingEnvironment _environment { get; }
        public CollectionFileService _cf { get; }
        AppSettings Configuration { get; set; }



        public TrackExport(DTrackService _DTrackService, IHostingEnvironment environment, CollectionFileService cf, IOptions<AppSettings> appsettings)
		{
            DTrackService = _DTrackService;
            _environment = environment;
            _cf = cf;
            Configuration = appsettings.Value;

        }

        static Newtonsoft.Json.JsonSerializer pascalCaseSerializer = Newtonsoft.Json.JsonSerializer.Create(
            new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            });

        public async Task<IEnumerable<TrackInfo>> Run()
		{

            var tracks = await DTrackService.GetAllTracks();

            var tracksEnum = tracks.Select(GetTrack);


            TrackInfo[] trackInfoList = await _cf.ProcessTrackInfo(tracksEnum.ToList());

            return trackInfoList;

        }

        public async Task<IEnumerable<TrackInfo>> Process()
        {

            String jsonFile = Path.Combine(_environment.WebRootPath, "musly") + Path.DirectorySeparatorChar + "all_tracks.json";

            string jsonData = File.ReadAllText(jsonFile);
            IEnumerable<Track> tracks = JsonSerializer.Deserialize<IEnumerable<Track>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var tracksEnum = tracks.Select(GetTrack);


            TrackInfo[] trackInfoList = await _cf.ProcessTrackInfo(tracksEnum.ToList());

            return trackInfoList;

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

