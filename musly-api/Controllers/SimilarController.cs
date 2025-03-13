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
        private TimbreService TimbreService { get; set; }

        public SimilarController(TimbreService timbreService)
        {
            TimbreService = timbreService;
        }

        [HttpGet]
        public IEnumerable<Song> GetSimilar(string track, int? count)
        {
            if (count == null)
            {
                count = 25;
            }

            return TimbreService.timbreSimilarity(track, count.Value);
        }
    }
}

