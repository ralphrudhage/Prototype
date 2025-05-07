namespace Model
{
    public class AttackCard : Card
    {
        public CardType type => CardType.ATTACK;
        public int cost { get; }
        public int damage;

        public AttackCard(int cost, int damage)
        {
            this.cost = cost;
            this.damage = damage;
        }
    }
}