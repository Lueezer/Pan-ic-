using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance { get; private set; }

    [Header("Prefabs dos Clientes (Homem e Mulher)")]
    [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>(); // Coloque os 2 prefabs aqui

    [Header("Pontos de Referência")]
    [SerializeField] private Transform doorPoint;      // Ponto onde nasce (Porta)
    [SerializeField] private Transform counterPoint;   // Ponto em frente ao balcão

    [Header("Configurações do Spawner")]
    [SerializeField] private float initialDelay = 10f; // Espera 10s no Play
    [SerializeField] private float spawnCooldown = 10f; // 10s após sair do balcão

    private bool isCounterOccupied = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Espera inicial de 10 segundos após dar Play
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Valida se o balcão está livre e se há pelo menos uma cadeira vaga no salão
            if (!isCounterOccupied && TableManager.Instance != null && TableManager.Instance.HasAvailableChair())
            {
                SpawnCustomer();
                isCounterOccupied = true;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefabs.Count == 0 || doorPoint == null || counterPoint == null)
        {
            Debug.LogWarning("CustomerSpawner: Faltam prefabs ou pontos configurados no Inspector!");
            return;
        }

        // Aleatoriza entre os prefabs disponíveis (Homem / Mulher)
        int randomIndex = Random.Range(0, customerPrefabs.Count);
        GameObject selectedPrefab = customerPrefabs[randomIndex];

        GameObject newCustomerObj = Instantiate(selectedPrefab, doorPoint.position, Quaternion.identity);
        Customer customer = newCustomerObj.GetComponent<Customer>();

        if (customer != null)
        {
            // Envia o ponto do balcão e da porta para o cliente andar até lá
            customer.SetupCustomer(counterPoint, doorPoint);
        }
    }

    // Chamado pelo cliente assim que ele sai do balcão em direção à cadeira
    public void NotifyCounterFreed()
    {
        StartCoroutine(FreeCounterCooldown());
    }

    private IEnumerator FreeCounterCooldown()
    {
        // Espera 10 segundos após o balcão ser liberado para permitir um novo spawn
        yield return new WaitForSeconds(spawnCooldown);
        isCounterOccupied = false;
    }
}