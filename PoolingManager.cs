using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager Instance;

    // 미리 등록된 프리팹 리스트 (키값은 프리팹 이름)
    public List<GameObject> poolPrefabs = new List<GameObject>();

    public Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private GameObject poolParent;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SetDefaultParent();
    }

    private void OnDisable()
    {
        Destroy(poolParent);
    }

    private void SetDefaultParent()
    {
        poolParent = new GameObject("PoolingParent");
    }

    // 풀 초기화 (key는 프리팹 이름으로 고정)
    public void SetDefaultPool(GameObject prefab, int count)
    {
        string key = prefab.name;
        if (poolDictionary.ContainsKey(key)) return;

        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent.transform);
            obj.name = key;
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(key, objectPool);
        if (!poolPrefabs.Contains(prefab)) poolPrefabs.Add(prefab);
    }

    public void Get(string key, Vector3 pos, Vector3 rot, float timer = 0)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with tag {key} doesn't exist");
            return;
        }

        GameObject objectToSpawn = null;

        if (poolDictionary[key].Count > 0)
        {
            objectToSpawn = poolDictionary[key].Dequeue();
        }
        else
        {
            GameObject prefab = poolPrefabs.Find(p => p.name == key);
            if (prefab == null)
            {
                Debug.LogError($"Prefab with name {key} not found in poolPrefabs.");
                return;
            }

            objectToSpawn = Instantiate(prefab, poolParent.transform);
            objectToSpawn.name = key;
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = pos;
        objectToSpawn.transform.eulerAngles = rot;

        if (timer > 0) StartCoroutine(ReturnAfterDelay(objectToSpawn, timer));
    }

    private IEnumerator ReturnAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(go);
    }

    public void Return(GameObject go)
    {
        string key = go.name;

        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Trying to return object {key} to non-existent pool.");
            Destroy(go);
            return;
        }

        go.SetActive(false);
        poolDictionary[key].Enqueue(go);
    }
}
