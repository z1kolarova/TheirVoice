using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityGenerator : MonoBehaviour
{
    public static PersonalityGenerator I => instance;
    static PersonalityGenerator instance;

    [Header("Traits")]
    public int MaxPatience = 10;
    public int MaxBaseAwareness = 100;
    public int MaxBaseCompassion = 100;

    [Header("Diet")]
    [Range(0, 100)] public int ChickenOdds;
    [Range(0, 100)] public int CowOdds;
    [Range(0, 100)] public int PigOdds;
    [Range(0, 100)] public int FishOdds;
    [Range(0, 100)] public int DairyOdds;
    [Range(0, 100)] public int EggsOdds;
    [Range(0, 100)] public int HoneyOdds;

    private static System.Random rng;
    private int rangeMax = 100;
    private List<(AnimalExploitationsInDiet, int)> _tupleList;
    private void Start()
    {
        instance = this;
        rng = new System.Random();
         _tupleList = new List<(AnimalExploitationsInDiet, int)>(){
            (AnimalExploitationsInDiet.Chicken, ChickenOdds),
            (AnimalExploitationsInDiet.Cow, CowOdds),
            (AnimalExploitationsInDiet.Pig, PigOdds),
            (AnimalExploitationsInDiet.Fish, FishOdds),
            (AnimalExploitationsInDiet.Dairy, DairyOdds),
            (AnimalExploitationsInDiet.Eggs, EggsOdds),
            (AnimalExploitationsInDiet.Honey, HoneyOdds),
        };
    }

    public PersonalityCore GetNewPersonality()
    {
        var diet = AddFlagsIfOdds(AnimalExploitationsInDiet.None);
        
        var pc = new PersonalityCore()
        {
            Traits = new Traits(rng.Next(MaxPatience), rng.Next(MaxBaseAwareness), rng.Next(MaxBaseCompassion)),
            Diet = diet
        };
        return pc;
    }

    private AnimalExploitationsInDiet AddFlagsIfOdds(AnimalExploitationsInDiet addingTo)
    {
        foreach (var tuple in _tupleList)
        {
            if (rng.Next(rangeMax) <= tuple.Item2)
            {
                addingTo = addingTo | tuple.Item1;
            }
        }
        return addingTo;
    }

}
