namespace Model
{
    public class MoveAction : GameAction
    {
        public ActionType type => ActionType.MOVE;
        public int cost { get; }
        public int range;

        public MoveAction(int cost, int range)
        {
            this.cost = cost;
            this.range = range;
        }
    }

}