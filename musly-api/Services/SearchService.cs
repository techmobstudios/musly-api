using System;
using Nest;
using System.Runtime;
using musly_api.Model.CMTZ;
using X.PagedList;

namespace musly_api.Services
{
	public class SearchService
	{
		public SearchService()
		{
		}

        public DSearchResult<Track> SearchTracks(string query, int currentPage, int itemsPerPage)
        {

            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node).DisableDirectStreaming();
            settings.DefaultIndex("tracks");

            var client = new ElasticClient(settings);
            var start = (currentPage - 1) * itemsPerPage;
            var finish = itemsPerPage;
            //var query = criteria.TrackTitle;

            var response = client.Search<Track>(s => s
                                  .Query(q => q.QueryString(qs => qs.Query(query).DefaultField("_all")
                                   ))
                                   .Sort(ss =>
                                       ss.Descending(SortSpecialField.Score)
                                   )
                                  .From(start)
                                  .Size(finish)
                              );

            var result = new DSearchResult<Track>
            {
                Items = response.Documents.ToPagedList(),
                Itemscount = (int)response.Total,
            };


            return result;

        }

    }
}

