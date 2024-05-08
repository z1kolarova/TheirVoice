using System.Collections.Generic;
using UnityEngine;

public class PasserbyModelManager : MonoBehaviour
{
    public static PasserbyModelManager I => instance;
    static PasserbyModelManager instance;

    public List<GameObject> models;
    public RuntimeAnimatorController animatorController;

    private List<GameObject> availablePool;
    private Dictionary<string, int> currentlyInScene;

    private void Start()
    {
        instance = this;
        availablePool = new List<GameObject>(models);

        currentlyInScene = new Dictionary<string, int>();
        foreach (var model in models)
        {
            currentlyInScene.Add(model.name, 0);
        }
    }

    public GameObject GetRandomModel()
    {

        var model = availablePool.Count > 0
            ? availablePool[RngUtils.Rng.Next(availablePool.Count)]
            : models[RngUtils.Rng.Next(models.Count)];

        if (availablePool.Count > 0)
        {
            availablePool.Remove(model);
        }

        currentlyInScene[model.name]++;

        return model;
    }

    public void RemoveFromModelsInScene(GameObject model)
    {
        currentlyInScene[model.name]--;
        if (currentlyInScene[model.name] == 0)
        {
            availablePool.Add(model);
        }
    }
}
