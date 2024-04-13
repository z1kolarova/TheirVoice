using Assets.Classes;
using System;

public class PersonalityCore
{
    public Prompt Prompt;
    public Traits Traits;
    public AnimalExploitationsInDiet Diet;

    public bool IsFullyPlantBased() => Diet == AnimalExploitationsInDiet.None;
    public bool EatsMeat() => Diet.HasFlag(AnimalExploitationsInDiet.Chicken)
        || Diet.HasFlag(AnimalExploitationsInDiet.Cow)
        || Diet.HasFlag(AnimalExploitationsInDiet.Pig)
        || Diet.HasFlag(AnimalExploitationsInDiet.Fish);
    public bool NotCowsButMilk() => !Diet.HasFlag(AnimalExploitationsInDiet.Cow)
        && Diet.HasFlag(AnimalExploitationsInDiet.Dairy);
}

public class Traits
{
    public int Patience { get; set; }
    public int Awareness { get; set; }
    public int Compassion { get; set; }
    public double Willingness() => Awareness * Compassion;

    public Traits(int patience, int awareness, int compassion)
    {
        Patience = patience;
        Awareness = awareness;
        Compassion = compassion;
    }

}

[Flags]
public enum AnimalExploitationsInDiet 
{
    None = 0,
    Chicken = 1 << 0,
    Cow = 1 << 1,
    Pig = 1 << 2,
    Fish = 1 << 3,
    Dairy = 1 << 4,
    Eggs = 1 << 5,
    Honey = 1 << 6,
    Eat = Chicken | Cow | Pig | Fish | Eggs | Honey,
    Drink = Dairy,
    LandAnimals = Chicken | Cow | Pig,
    Flesh = Chicken | Cow | Pig | Fish,
}