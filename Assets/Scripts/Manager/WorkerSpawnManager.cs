using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSpawnManager : Singleton<WorkerSpawnManager>
{
    [SerializeField] private List<GameObject> workerPrefab;
    [SerializeField] private Transform spawnPoints;
    public void SpawnWorkers(int id)
    {
        Instantiate(workerPrefab[id], spawnPoints.position, Quaternion.identity);
    }
}
