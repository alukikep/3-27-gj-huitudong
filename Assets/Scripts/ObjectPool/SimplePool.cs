using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public static SimplePool Instance;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<int, GameObject> instanceToPrefabMap = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary.Add(prefab, new Queue<GameObject>());
        }
        GameObject objToSpawn;

        if (poolDictionary[prefab].Count > 0)
        {
            objToSpawn = poolDictionary[prefab].Dequeue();
        }
        else
        {
            objToSpawn = Instantiate(prefab);
            instanceToPrefabMap.Add(objToSpawn.GetInstanceID(), prefab);
            objToSpawn.transform.SetParent(this.transform);
        }
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);
        return objToSpawn;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;
        int id = obj.GetInstanceID();

        if (instanceToPrefabMap.ContainsKey(id))
        {
            GameObject originalPrefab = instanceToPrefabMap[id];
            obj.SetActive(false);
            poolDictionary[originalPrefab].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    public void Despawn(GameObject obj, float delay)
    {
        StartCoroutine(DespawnCoroutine(obj, delay));
    }

    private System.Collections.IEnumerator DespawnCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }

}
