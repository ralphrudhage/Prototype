namespace Model.PriestCards
{
    public class Wand : Card
    {
        public CardType type => CardType.RANGED;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }

        public Wand(string name, int cost, int effect, int range)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
        }
    }
}