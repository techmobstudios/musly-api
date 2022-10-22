using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using musly_api.Helpers;
using musly_api.Services;

namespace musly_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendController : ControllerBase
    {
        private MuslyService MuslyService { get; set; }

        public RecommendController(MuslyService muslyService)
        {
            MuslyService = muslyService;
        }

        /*[HttpGet]
        public String Get(string track, int? count)
        {
            if(count == null)
            {
                count = 25;
            }

            var command = "musly -k "+ count + " -p '"+ track + "' -c /home/music/collection.musly -j /home/music/jukebox";
            var results = command.Bash();
            Console.WriteLine("Results: " + results);
            string[] console = Regex.Split(results, ".*Computing the k=.* most similar tracks to.*mp3");
            results= console[1];
            results = results.Replace("/home/music", "https://www.certifiedmixtapez.com/UploadedFiles/Mixtapes");
            return results;
        }*/

        [HttpGet]
        public IEnumerable<Song> GetRecommendations(string track, int? count)
        {
            if (count == null)
            {
                count = 25;
            }

            //MuslyService.timbreSimilarity(track, count.Value);

            return MuslyService.getTrackRecommends(track, count.Value);
        }
    }
}
