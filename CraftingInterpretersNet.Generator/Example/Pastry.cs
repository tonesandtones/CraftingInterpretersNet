using System.Collections.Generic;

namespace CraftingInterpretersNet.Generator.Example;

public abstract class Pastry
{
    public abstract T Accept<T>(Visitor<T> visitor);
    
    public interface Visitor<out T>
    {
        T VisitDoughnut(Doughnut pastry);
        T VisitCroissant(Croissant pastry);
    }

    public class Doughnut : Pastry
    {
        public string Colour { get; }
        public bool Holey { get; }

        public Doughnut(string colour, bool holey)
        {
            Colour = colour;
            Holey = holey;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitDoughnut(this);
        }
    }

    public class Croissant : Pastry
    {
        public List<string> Fillings { get; }

        public Croissant(List<string> fillings)
        {
            Fillings = fillings;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitCroissant(this);
        }
    }
}