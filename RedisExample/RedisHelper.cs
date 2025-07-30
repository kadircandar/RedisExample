using StackExchange.Redis;
using System.Text.Json;

namespace RedisExample
{
    public class RedisHelper
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly IServer _server;

        public RedisHelper(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();

            var endpoint = _redis.GetEndPoints().First();
            _server = _redis.GetServer(endpoint); //Used to execute Redis server-level commands (e.g., Keys, FlushDb, Info).
        }

        // -------------------- STRING -------------------- //
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
            => await _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetAsync(string key)
            => await _db.StringGetAsync(key);


        public async Task<bool> SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            string json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(key, json, expiry);
        }

        public async Task<T?> GetObjectAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
        }

        // -------------------- DELETE -------------------- //
        public async Task<bool> DeleteAsync(string key)
            => await _db.KeyDeleteAsync(key);

        public async Task<long> DeleteKeysAsync(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            return await _db.KeyDeleteAsync(redisKeys);
        }

        public async Task<long> DeleteByPatternAsync(string pattern)
        {
            var keys = _server.Keys(pattern: pattern).ToArray();
                return await DeleteKeysAsync(keys.Select(k => (string)k));
        }

        public async Task<bool> ExistsAsync(string key)
            => await _db.KeyExistsAsync(key);

        // -------------------- HASH --------------------

        public async Task<bool> HashSetAsync(string key, string field, string value)
            => await _db.HashSetAsync(key, field, value);

        public async Task<string?> HashGetAsync(string key, string field)
        {
            var value = await _db.HashGetAsync(key, field);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task HashSetMultipleAsync(string key, Dictionary<string, string> values)
        {
            var entries = values.Select(v => new HashEntry(v.Key, v.Value)).ToArray();
            await _db.HashSetAsync(key, entries);
        }


        public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            var entries = await _db.HashGetAllAsync(key);
            return entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        }


        public async Task<bool> HashDeleteFieldAsync(string key, string field)
            => await _db.HashDeleteAsync(key, field);

        public async Task<long> HashLengthAsync(string key)
            => await _db.HashLengthAsync(key);

        public async Task<bool> HashFieldExistsAsync(string key, string field)
            => await _db.HashExistsAsync(key, field);

    }
}
