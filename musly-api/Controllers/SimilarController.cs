using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using musly_api.Services;

namespace musly_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimilarController : ControllerBase
    {
        private MuslyService MuslyService { get; set; }

        public SimilarController(MuslyService muslyService)
        {
            MuslyService = muslyService;
        }

        [HttpGet]
        public IEnumerable<Song> GetSimilar(string track, int? count)
        {
            if (count == null)
            {
                count = 25;
            }

            return MuslyService.timbreSimilarity(track, count.Value);
        }
    }
}

