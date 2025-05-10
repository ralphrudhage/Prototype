namespace Model.WarriorCards
{
    public class Charge : Card
    {
        public CardType type => CardType.MOVE;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }
        
        public Charge(string name, int cost, int effect, int range)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
        }
    }
}