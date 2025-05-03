namespace Model
{
    public interface GameAction
    {
        ActionType type { get; }
        int cost { get; }
    }
}