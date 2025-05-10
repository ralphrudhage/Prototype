namespace Model.WarriorCards
{
    public class Strike : Card
    {
        public CardType type => CardType.MELEE;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }

        public Strike(string name, int cost, int effect, int range)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
        }
    }
}