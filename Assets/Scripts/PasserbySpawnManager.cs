using System.Collections.Generic;
using UnityEngine;

public class PasserBySpawnManager : MonoBehaviour
{
    public static PasserBySpawnManager I => instance;
    static PasserBySpawnManager instance;

    private List<PasserbyAI> passerbyList;

    private const int PASSERBY_AMOUNT = 12;
    private const float SPAWN_DELAY = 1f;
    
    [SerializeField]
    private PasserbyAI passerbyAIPrefab;
    [SerializeField]
    private Transform passerbySpawn;
    [SerializeField]
    private Transform passerbyTarget;

    private float timeSinceLastSpawn = 0f;

    private void Start() {
        instance = this;
        passerbyList = new List<PasserbyAI>();
    }

    private void Update() {
        if (timeSinceLastSpawn < SPAWN_DELAY) {
            timeSinceLastSpawn += Time.deltaTime;
            return;
        }
        if (passerbyList.Count < PASSERBY_AMOUNT) {
            var passerbyAI = GameObject.Instantiate(passerbyAIPrefab, passerbySpawn.transform);
            passerbyAI.target = passerbyTarget;
            passerbyList.Add(passerbyAI);
            timeSinceLastSpawn = 0f;
        }
    }

    public void Remove(PasserbyAI passerbyAI) {
        passerbyList.Remove(passerbyAI);
    }
}
