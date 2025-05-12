namespace Model.WarriorCards
{
    public class Block : Card
    {
        public CardType type => CardType.DEFENSE;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }
        
        public Block(string name, int cost, int effect, int range)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
        }
    }
}