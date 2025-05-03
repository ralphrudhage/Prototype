namespace Model
{
    public class AttackAction : GameAction
    {
        public ActionType type => ActionType.ATTACK;
        public int cost { get; }
        public int damage;

        public AttackAction(int cost, int damage)
        {
            this.cost = cost;
            this.damage = damage;
        }
    }
}