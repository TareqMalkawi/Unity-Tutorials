using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public string name;
    public GameObject prefab;
    public Transform parent;
    public int amount;
    public bool expandable;

    public PoolItem(string name, GameObject prefab, Transform parent, int amount, bool expandable)
    {
        this.name = name;
        this.prefab = prefab;
        this.parent = parent;
        this.amount = amount;
        this.expandable = expandable;
    }
}

public class Pool : MonoBehaviour
{
    public static Pool instance;

    [HideInInspector] public List<PoolItem> items;
    private List<GameObject> pooledItems;

    private void Awake() => instance = this;

    private void Start()
    {
        pooledItems = new List<GameObject>();

        foreach (PoolItem item in items)
        {
            for (int i = 0; i < item.amount; i++)
            {
                GameObject obj = Instantiate(item.prefab, item.parent);
                obj.SetActive(false);
                pooledItems.Add(obj);
            }
        }
    }

    public GameObject Get(string tag)
    {
        for (int i = 0; i < pooledItems.Count; i++)
        {
            if (!pooledItems[i].activeInHierarchy && pooledItems[i].CompareTag(tag))
            {
                pooledItems[i].SetActive(true);
                return pooledItems[i];
            }
        }

        foreach (PoolItem item in items)
        {
            if (item.prefab.CompareTag(tag) && item.expandable)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                pooledItems.Add(obj);
                return obj;
            }
        }

        return null;
    }

    public void Return(GameObject item)
    {
        item.SetActive(false);
    }
}