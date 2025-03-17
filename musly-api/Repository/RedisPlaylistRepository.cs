using System;
using musly_api.Model.CMTZ;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using musly_api.Model;

namespace musly_api.Repository
{

    public class RedisPlaylistRepository
	{
        private readonly IConnectionMultiplexer _redis;

        private const string TrackPrefix = "playlist:";

        public RedisPlaylistRepository(IConnectionMultiplexer redis)
		{
            _redis = redis;

        }

        public async Task<Playlist> GetPlaylistById(string id)
        {
            var db = _redis.GetDatabase();
            var trackJson = await db.StringGetAsync($"{TrackPrefix}{id}");
            return trackJson.IsNull ? null : JsonSerializer.Deserialize<Playlist>(trackJson);
        }

        public async Task<IEnumerable<Playlist>> GetAllTracks()
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var db = _redis.GetDatabase();
            var keys = _redis.GetServer(_redis.GetEndPoints().First())
                .Keys(pattern: $"{TrackPrefix}*");

            var tasks = keys.Select(key => db.StringGetAsync(key));
            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => !r.IsNull)
                .Select(r =>
                    JsonSerializer.Deserialize<Playlist>(r, options)
                    );
        }

        public async Task SaveTrack(Playlist track)
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(track);

            await Task.WhenAll(
                db.StringSetAsync($"{TrackPrefix}{track.Id}", json)
            );
        }
    }
}

