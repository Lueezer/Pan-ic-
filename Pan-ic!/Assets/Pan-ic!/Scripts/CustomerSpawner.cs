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
    [SerializeField] private bool isInfiniteMode = true; // Se ativado, continua gerando sem limite
    [SerializeField] private int maxCustomersTotal = 10;  // Usado caso não seja modo infinito
    [SerializeField] private float initialDelay = 5f;     // Tempo para o primeiro cliente chegar
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
        // Espera inicial do começo da fase
        yield return new WaitForSeconds(initialDelay);

        while (isInfiniteMode || spawnedCount < maxCustomersTotal)
        {
            // 1. Checa se o balcão tá livre
            bool isCounterFree = (currentActiveCustomer == null);

            // 2. Checa se existe cadeira livre no restaurante
            bool hasFreeChair = true;
            if (TableManager.Instance != null)
            {
                hasFreeChair = TableManager.Instance.HasFreeChair();
            }

            // Só gera se O BALCÃO TIVER LIVRE + TIVER CADEIRA DISPONÍVEL
            if (isCounterFree && hasFreeChair)
            {
                // Espera o tempo de 10 segundos antes do novo cliente aparecer
                yield return new WaitForSeconds(delayBetweenCustomers);

                // Dupla checagem antes de instanciar (para garantir que nada mudou durante os 10s)
                if (currentActiveCustomer == null && (TableManager.Instance == null || TableManager.Instance.HasFreeChair()))
                {
                    SpawnRandomCustomer();
                    spawnedCount++;
                }
            }
            else
            {
                // Se a casa tiver cheia ou o balcão ocupado, checa novamente a cada 1 segundo
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

    // Chamado pelo Customer.cs assim que ele sai do balcão e caminha até a mesa ou vai embora
    public void ClearCurrentCustomer()
    {
        currentActiveCustomer = null;
    }
}