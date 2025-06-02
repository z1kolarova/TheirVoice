using System;

namespace Assets.Enums
{
    [Flags]
    public enum ArgumentationTag
    {
        None = 0,
        Troll = 1 << 0,
        Vegetarian = 1 << 1,
        Nutrition = 1 << 2,
        HumanSuperiority = 1 << 3,
        SystemsFault = 1 << 4,
        Reducing = 1 << 5,
        HumaneFarming = 1 << 6,
        ItsNature = 1 << 7,
        Taste = 1 << 8,
        ItsHard = 1 << 9,
        Religion = 1 << 10,
        Lions = 1 << 11,
        PlantsFeelPain = 1 << 12,
        CropDeaths = 1 << 13,
        Expensive = 1 << 14,
    }
}

