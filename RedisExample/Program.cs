using RedisExample;
using RedisExample.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<RedisHelper>(sp =>
{
    string redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return new RedisHelper(redisConnection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 


app.MapGet("/cache/set/{key}/{value}", async (string key, string value, RedisHelper redis) =>
{
    await redis.SetAsync(key, value, TimeSpan.FromMinutes(10));
    return Results.Ok($"Key '{key}' saved with value '{value}'");
})
.WithName("SetCacheValue");


app.MapGet("/cache/get/{key}", async (string key, RedisHelper redis) =>
{
    var value = await redis.GetAsync(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
})
.WithName("GetCacheValue");


app.MapDelete("/cache/delete/{key}", async (string key, RedisHelper redis) =>
{
    bool deleted = await redis.DeleteAsync(key);
    return deleted ? Results.Ok($"Key '{key}' deleted") : Results.NotFound();
})
.WithName("DeleteCacheKey");


app.MapDelete("/cache/delete-pattern/{pattern}", async (string pattern, RedisHelper redis) =>
{
    long count = await redis.DeleteByPatternAsync(pattern);
    return Results.Ok($"{count} keys deleted with pattern '{pattern}'");
})
.WithName("DeleteCacheByPattern")
.WithDescription("Deletes all keys from Redis that match the specified pattern. Example: 'user:*' will delete all keys starting with 'user:'.")
.WithOpenApi();


app.MapPost("/product/save", async (Product product, RedisHelper redis) =>
{
    string key = $"product:{product.Id}";
    bool result = await redis.SetObjectAsync(key, product, TimeSpan.FromMinutes(30));
    return result ? Results.Ok($"Product saved with key: {key}") : Results.Problem("Failed to save");
})
.WithName("SaveProduct");


app.MapGet("/product/{id}", async (int id, RedisHelper redis) =>
{
    string key = $"product:{id}";
    var product = await redis.GetObjectAsync<Product>(key);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct");


app.MapDelete("/product/{id}", async (int id, RedisHelper redis) =>
{
    string key = $"product:{id}";
    bool deleted = await redis.DeleteAsync(key);
    return deleted ? Results.Ok("Deleted") : Results.NotFound();
})
.WithName("DeleteProduct");


app.MapPost("/hash/set/{key}/{field}/{value}", async (string key, string field, string value, RedisHelper redis) =>
{
    await redis.HashSetAsync(key, field, value);
    return Results.Ok($"Hash key '{key}' field '{field}' set to '{value}'");
})
.WithName("SetHashField");


app.MapGet("/hash/get/{key}/{field}", async (string key, string field, RedisHelper redis) =>
{
    var value = await redis.HashGetAsync(key, field);
    return value is not null ? Results.Ok(value) : Results.NotFound();
})
.WithName("GetHashField");


app.MapGet("/hash/all/{key}", async (string key, RedisHelper redis) =>
{
    var data = await redis.HashGetAllAsync(key);
    return Results.Ok(data);
})
.WithName("GetAllHashFields");


app.MapPost("/leaderboard/{player}/{score}", async (string player, double score, RedisHelper redis) =>
{
    await redis.AddOrUpdatePlayerAsync(player, score);
    return Results.Ok(new { Message = $"{player}'s score has been updated to {score}." });
})
.WithName("AddOrUpdatePlayer");


app.MapGet("/leaderboard", async (RedisHelper redis) =>
{
    var players = await redis.GetAllAsync();
    var response = players.Select((p, index) => new PlayerInfo(p.Player, p.Score, index + 1));
    return Results.Ok(response);
})
.WithName("GetAllPlayers");


app.MapGet("/leaderboard/{player}", async (string player, RedisHelper redis) =>
{
    var (score, rank) = await redis.GetPlayerAsync(player);
    if (score.HasValue && rank.HasValue)
    {
        return Results.Ok(new PlayerInfo(player, score.Value, rank));
    }
    return Results.NotFound(new { Message = "Player not found." });
})
.WithName("GetPlayer");


app.MapGet("/leaderboard/top/{count:int}", async (int count, RedisHelper redis) =>
{
    var players = await redis.GetTopAsync(count);
    var response = players.Select((p, index) => new PlayerInfo(p.Player, p.Score, index + 1));
    return Results.Ok(response);
})
.WithName("GetTopPlayers");



app.Run();
