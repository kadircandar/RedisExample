namespace RedisExample.Models
{
    public record PlayerInfo(string Player, double Score, long? Rank = null);
}
