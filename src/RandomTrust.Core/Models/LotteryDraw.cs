namespace RandomTrust.Core.Models;

public class LotteryDraw
{
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public byte[] Seed { get; set; } = Array.Empty<byte>();
    public byte[] GeneratorState { get; set; } = Array.Empty<byte>();
    public DateTime DrawTime { get; set; }
    public string DrawId { get; set; } = Guid.NewGuid().ToString();
    
    public static LotteryDraw Create(int[] numbers, byte[] seed, byte[] state)
    {
        return new LotteryDraw
        {
            Numbers = numbers,
            Seed = seed,
            GeneratorState = state,
            DrawTime = DateTime.UtcNow
        };
    }
}
