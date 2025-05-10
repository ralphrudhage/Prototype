namespace Model.MageCards
{
    public class Curse : Card
    {
        public CardType type => CardType.DOT;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }

        public int duration;

        public Curse(string name, int cost, int effect, int range, int duration)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
            this.duration = duration;
        }
    }
}