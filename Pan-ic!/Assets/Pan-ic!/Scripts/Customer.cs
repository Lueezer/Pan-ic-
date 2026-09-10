using System.Collections;
using UnityEngine;

public enum CustomerState
{
    WalkingToCounter,
    WaitingToOrder,
    ShowingOrder,
    WalkingToSeat,
    WaitingForFood,
    Eating,
    Leaving
}

public class Customer : MonoBehaviour
{
    [Header("Velocidade e Movimento")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("UI do Balão / Indicadores (Filhos na Hierarchy)")]
    [Tooltip("Objeto visual da Exclamação (!)")]
    [SerializeField] private GameObject exclamationBubble;

    [Tooltip("Objeto visual do Balão de Fala com o Pão de Queijo")]
    [SerializeField] private GameObject orderBubble;

    [Header("Pontos de Destino (Configurados na Cena)")]
    private Transform doorPoint;
    private Transform counterPoint;
    private TableManager.ChairSlot assignedChairSlot;

    private CustomerState currentState = CustomerState.WalkingToCounter;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupCustomer(Transform spawnDoor, Transform targetCounter)
    {
        doorPoint = spawnDoor;
        counterPoint = targetCounter;

        transform.position = doorPoint.position;

        if (exclamationBubble != null) exclamationBubble.SetActive(false);
        if (orderBubble != null) orderBubble.SetActive(false);

        currentState = CustomerState.WalkingToCounter;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkingToCounter:
                if (counterPoint != null)
                {
                    MoveTowards(counterPoint.position, () =>
                    {
                        // Chegou no balcão
                        currentState = CustomerState.WaitingToOrder;
                        if (exclamationBubble != null) exclamationBubble.SetActive(true);
                    });
                }
                break;

            case CustomerState.WaitingToOrder:
                // Aguarda o jogador interagir no balcão (tecla 'E')
                break;

            case CustomerState.WalkingToSeat:
                if (assignedChairSlot != null)
                {
                    // Move até a posição da cadeira sorteada no TableManager
                    MoveTowards(assignedChairSlot.position, () =>
                    {
                        StartCoroutine(EatAndLeaveRoutine());
                    });
                }
                break;

            case CustomerState.Leaving:
                if (doorPoint != null)
                {
                    MoveTowards(doorPoint.position, () =>
                    {
                        // Chegou na porta de saída: libera a cadeira e destrói o NPC
                        if (assignedChairSlot != null && TableManager.Instance != null)
                        {
                            TableManager.Instance.ReleaseChair(assignedChairSlot);
                        }
                        Destroy(gameObject);
                    });
                }
                break;
        }
    }

    /// <summary>
    /// Chamado pelo PlayerController ao pressionar 'E' no alcance do cliente.
    /// </summary>
    public void InteractWithCustomer()
    {
        if (currentState == CustomerState.WaitingToOrder)
        {
            StartCoroutine(OrderRoutine());
        }
    }

    private IEnumerator OrderRoutine()
    {
        currentState = CustomerState.ShowingOrder;

        // Esconde '!' e mostra balão do Pão de Queijo
        if (exclamationBubble != null) exclamationBubble.SetActive(false);
        if (orderBubble != null) orderBubble.SetActive(true);

        // Aguarda 5 segundos mostrando o pedido no balcão
        yield return new WaitForSeconds(5f);

        if (orderBubble != null) orderBubble.SetActive(false);

        // Procura uma cadeira disponível no TableManager
        if (TableManager.Instance != null)
        {
            assignedChairSlot = TableManager.Instance.GetRandomFreeChair();
        }

        // Se encontrou cadeira livre, caminha até ela. Se não, volta para a porta.
        if (assignedChairSlot != null)
        {
            // Libera o balcão para o spawner contar o tempo do próximo cliente
            CustomerSpawner spawner = FindAnyObjectByType<CustomerSpawner>();
            if (spawner != null) spawner.ClearCurrentCustomer();

            currentState = CustomerState.WalkingToSeat;
        }
        else
        {
            CustomerSpawner spawner = FindAnyObjectByType<CustomerSpawner>();
            if (spawner != null) spawner.ClearCurrentCustomer();

            currentState = CustomerState.Leaving;
        }
    }

    private IEnumerator EatAndLeaveRoutine()
    {
        currentState = CustomerState.Eating;
        Debug.Log("[Cliente] Sentou e está comendo o pão de queijo...");

        // Permanece 5 segundos simulando que está comendo
        yield return new WaitForSeconds(5f);

        Debug.Log("[Cliente] Terminou de comer. Voltando para a saída!");
        currentState = CustomerState.Leaving;
    }

    private void MoveTowards(Vector3 targetPosition, System.Action onArrival)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Inverte o sprite horizontalmente conforme a direção do movimento
        if (spriteRenderer != null)
        {
            if (targetPosition.x < transform.position.x)
                spriteRenderer.flipX = true;
            else if (targetPosition.x > transform.position.x)
                spriteRenderer.flipX = false;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            onArrival?.Invoke();
        }
    }

    public CustomerState GetCurrentState() => currentState;
}