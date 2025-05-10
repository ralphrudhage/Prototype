namespace Model
{
    public interface Card
    {
        CardType type { get; }
        int cost { get; }
        int range { get; }
        string name { get; }
        int effect { get; }
    }
}