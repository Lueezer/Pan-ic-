using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Prefabs de Clientes")]
    [SerializeField] private GameObject maleCustomerPrefab;
    [SerializeField] private GameObject femaleCustomerPrefab;

    [Header("Pontos do Cenário")]
    [SerializeField] private Transform doorPoint;
    [SerializeField] private Transform counterPoint;

    [Header("Configurações do Protótipo")]
    [SerializeField] private bool isInfiniteMode = true;
    [SerializeField] private int maxCustomersTotal = 10;
    [SerializeField] private float initialDelay = 5f;          // 5s para o 1º cliente da fase
    [SerializeField] private float delayBetweenCustomers = 10f; // 10s após liberar o balcão

    private int spawnedCount = 0;
    private Customer currentActiveCustomer = null;

    private void Start()
    {
        if (doorPoint == null) doorPoint = transform;
        StartCoroutine(SpawnLoopRoutine());
    }

    private IEnumerator SpawnLoopRoutine()
    {
        // 1. Espera 5 segundos APENAS no começo do jogo
        yield return new WaitForSeconds(initialDelay);

        while (isInfiniteMode || spawnedCount < maxCustomersTotal)
        {
            bool isCounterFree = (currentActiveCustomer == null);

            bool hasFreeChair = true;
            if (TableManager.Instance != null)
            {
                hasFreeChair = TableManager.Instance.HasFreeChair();
            }

            // Se o balcão está livre e tem cadeira, gera o cliente!
            if (isCounterFree && hasFreeChair)
            {
                SpawnRandomCustomer();
                spawnedCount++;

                // Espera o cliente sair do balcão (enquanto o balcão estiver ocupado por ele)
                while (currentActiveCustomer != null)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                // Assim que ele saiu do balcão, aguarda os 10 segundos antes de spawnar o próximo!
                yield return new WaitForSeconds(delayBetweenCustomers);
            }
            else
            {
                // Se não tiver cadeira livre, aguarda 1s para checar de novo
                yield return new WaitForSeconds(1f);
            }
        }
    }

    public void SpawnRandomCustomer()
    {
        if (maleCustomerPrefab == null || femaleCustomerPrefab == null || counterPoint == null) return;

        GameObject selectedPrefab = (Random.value > 0.5f) ? maleCustomerPrefab : femaleCustomerPrefab;
        GameObject newCustomerObj = Instantiate(selectedPrefab, doorPoint.position, Quaternion.identity);

        currentActiveCustomer = newCustomerObj.GetComponent<Customer>();

        if (currentActiveCustomer != null)
        {
            currentActiveCustomer.SetupCustomer(doorPoint, counterPoint);
        }
    }

    public void ClearCurrentCustomer()
    {
        currentActiveCustomer = null;
    }
}