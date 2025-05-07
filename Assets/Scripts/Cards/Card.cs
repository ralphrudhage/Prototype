namespace Model
{
    public interface Card
    {
        CardType type { get; }
        int cost { get; }
    }
}