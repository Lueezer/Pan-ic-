using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner instance;
    public GameObject customerPrefab;
    public float spawnDelay = 5f;

    private bool hasActiveCustomer = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        if (!hasActiveCustomer)
        {
            hasActiveCustomer = true;
            Instantiate(customerPrefab);
        }
    }

    public void OnCustomerLeft()
    {
        hasActiveCustomer = false;
        StartCoroutine(WaitAndSpawnNext());
    }

    private IEnumerator WaitAndSpawnNext()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnCustomer();
    }
}