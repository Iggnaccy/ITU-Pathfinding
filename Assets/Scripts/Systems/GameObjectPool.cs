using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    private GameObject pooledObject;

    private int poolSize = 10;

    private List<GameObject> pool = new List<GameObject>();

    public void Initialize(GameObject prefab, int size)
    {
        ClearPool();
        if(prefab == null)
        {
            Debug.LogError("Prefab cannot be null");
            return;
        }
        pooledObject = prefab;
        ExpandPool(size);
        poolSize = size;
    }

    public GameObject GetPooledObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        ExpandPool(1);
        GameObject newObj = pool[pool.Count - 1];
        newObj.SetActive(true);
        return newObj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = this.transform;
        if (!pool.Contains(obj))
        {
            pool.Add(obj);
        }
    }

    private void ExpandPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(pooledObject);
            obj.SetActive(false);
            obj.transform.parent = this.transform;
            pool.Add(obj);
        }
        poolSize += count;
    }

    private void ClearPool()
    {
        foreach (var obj in pool)
        {
            Destroy(obj);
        }
        pool.Clear();
        poolSize = 0;
    }
}
