using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BloodSpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnParent;
    [SerializeField] private GameObject bloodPrefab;
    
    private const int poolSize = 1000;
    private const int maxPoolSize = 1500;
    private readonly List<GameObject> bloodPooledObjects = new();

    private void Start()
    {
        for (var i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(bloodPrefab, spawnParent.transform);
            obj.SetActive(false);
            bloodPooledObjects.Add(obj);
        }
    }
    
    private void Reset()
    {
        CheckAndShrinkPool();
    }
    
    private void CheckAndShrinkPool()
    {
        if (bloodPooledObjects.Count > maxPoolSize)
        {
            for (var i = bloodPooledObjects.Count - 1; i >= maxPoolSize; i--)
            {
                if (!bloodPooledObjects[i].activeInHierarchy)
                {
                    Destroy(bloodPooledObjects[i]);
                    bloodPooledObjects.RemoveAt(i);
                }
            }
        }
    }
    
    public GameObject SpawnBlood(Vector3 targetPosition, int sortingOrder)
    {
        foreach (var blood in bloodPooledObjects.Where(t => !t.activeInHierarchy))
        {
            blood.transform.position = targetPosition;
            if (sortingOrder > 0)
            {
                blood.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            }
            return blood;
        }
        
        var obj = Instantiate(bloodPrefab, spawnParent.transform);
        if (sortingOrder > 0)
        {
            obj.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        }
        obj.transform.position = targetPosition;
        obj.SetActive(false);
        
        bloodPooledObjects.Add(obj);
        return obj;
    }
}
