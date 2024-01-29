using System;
using System.Collections.Generic;
using Assets.Classes;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PasserbyModelManager : MonoBehaviour
{
    public static PasserbyModelManager I => instance;
    static PasserbyModelManager instance;

    public List<GameObject> models;
    public RuntimeAnimatorController animatorController;
    private static System.Random rng;


    private void Start() {
        instance = this;
        rng = new System.Random();
    }

    public GameObject GetRandomModel() {
        return models[rng.Next(models.Count)];
    }
}
