namespace Model.PriestCards
{
    public class Heal : Card
    {
        public CardType type => CardType.PARTY;
        public int cost { get; }
        public int range { get; }
        public string name { get; }
        public int effect { get; }

        public Heal(string name, int cost, int effect, int range)
        {
            this.name = name;
            this.cost = cost;
            this.effect = effect;
            this.range = range;
        }
    }
}