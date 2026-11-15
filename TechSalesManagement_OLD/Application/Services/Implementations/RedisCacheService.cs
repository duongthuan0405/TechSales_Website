using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TechSalesManagement.Application.Services.Interfaces;

namespace TechSalesManagement.Application.Services.Implementations;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly int _defaultCacheDurationInMinutes;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger, IConfiguration configuration)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = _connectionMultiplexer.GetDatabase();
        _logger = logger;

        // Use IConfiguration to retrieve settings (supporting double underscore fallback)
        var durationStr = configuration["Redis:CacheDurationInMinutes"] ?? configuration["Redis__CacheDurationInMinutes"];
        if (int.TryParse(durationStr, out var minutes))
        {
            _defaultCacheDurationInMinutes = minutes;
        }
        else
        {
            _defaultCacheDurationInMinutes = 15;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        var ttl = expiration ?? TimeSpan.FromMinutes(_defaultCacheDurationInMinutes);
        await _database.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                var keys = server.Keys(pattern: prefix + "*");
                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning keys on endpoint {Endpoint}", endpoint);
            }
        }
    }
}
