namespace Model
{
    public class MoveCard : Card
    {
        public CardType type => CardType.MOVE;
        public int cost { get; }
        public int range;

        public MoveCard(int cost, int range)
        {
            this.cost = cost;
            this.range = range;
        }
    }

}