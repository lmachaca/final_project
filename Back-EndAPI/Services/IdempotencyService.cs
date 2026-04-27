using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Back_EndAPI.Services
{
    public interface IIdempotencyService
    {
        Task<(bool IsIdempotent, string? CachedResponse)> CheckIdempotencyAsync(string idempotencyKey);
        Task CacheResponseAsync(string idempotencyKey, string response);
    }

    public class IdempotencyService : IIdempotencyService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(IDistributedCache cache, ILogger<IdempotencyService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<(bool IsIdempotent, string? CachedResponse)> CheckIdempotencyAsync(string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return (false, null);

            var cachedResponse = await _cache.GetStringAsync(idempotencyKey);

            if (cachedResponse != null)
            {
                _logger.LogInformation($"Idempotent request detected: {idempotencyKey}");
                return (true, cachedResponse);
            }
            return (false, null);
        }

        public async Task CacheResponseAsync(string idempotencyKey, string response)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return;

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };

            await _cache.SetStringAsync(idempotencyKey, response, cacheOptions);
            _logger.LogInformation($"Cached response for idempotency key: {idempotencyKey}");
        }
    }
}
