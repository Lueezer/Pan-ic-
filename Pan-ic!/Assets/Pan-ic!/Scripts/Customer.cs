using System.Collections;
using UnityEngine;

public enum CustomerState
{
    WalkingToCounter,
    WaitingToOrder,
    ShowingOrder,
    WalkingToSeat,
    WaitingForFoodAtTable, // Fica sentado esperando a comida chegar na mesa
    Eating,
    Leaving
}

public class Customer : MonoBehaviour
{
    [Header("Velocidade e Movimento")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("UI do Balão / Indicadores")]
    [SerializeField] private GameObject exclamationBubble;
    [SerializeField] private GameObject orderBubble;

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
                        currentState = CustomerState.WaitingToOrder;
                        if (exclamationBubble != null) exclamationBubble.SetActive(true);
                    });
                }
                break;

            case CustomerState.WalkingToSeat:
                if (assignedChairSlot != null)
                {
                    MoveTowards(assignedChairSlot.position, () =>
                    {
                        // Chegou na cadeira: FICA ESPERANDO A COMIDA! Não vai embora sozinho.
                        currentState = CustomerState.WaitingForFoodAtTable;
                    });
                }
                break;

            case CustomerState.Leaving:
                if (doorPoint != null)
                {
                    MoveTowards(doorPoint.position, () =>
                    {
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

    // Chamado pelo Player quando ele anota o pedido no balcão
    public void TakeOrder()
    {
        if (currentState == CustomerState.WaitingToOrder)
        {
            StartCoroutine(OrderRoutine());
        }
    }

    private IEnumerator OrderRoutine()
    {
        currentState = CustomerState.ShowingOrder;

        if (exclamationBubble != null) exclamationBubble.SetActive(false);
        if (orderBubble != null) orderBubble.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (orderBubble != null) orderBubble.SetActive(false);

        // Reserva a cadeira e libera o balcão
        if (TableManager.Instance != null)
        {
            assignedChairSlot = TableManager.Instance.GetRandomFreeChair();
        }

        CustomerSpawner spawner = FindAnyObjectByType<CustomerSpawner>();
        if (spawner != null) spawner.ClearCurrentCustomer();

        if (assignedChairSlot != null)
        {
            currentState = CustomerState.WalkingToSeat;
        }
        else
        {
            currentState = CustomerState.Leaving;
        }
    }

    // Chamado quando o jogador coloca o prato com pão de queijo na mesa do cliente
    public void ServeFood(GameObject plateObject)
    {
        if (currentState == CustomerState.WaitingForFoodAtTable)
        {
            StartCoroutine(EatRoutine(plateObject));
        }
    }

    private IEnumerator EatRoutine(GameObject plateObject)
    {
        currentState = CustomerState.Eating;

        if (assignedChairSlot != null && plateObject != null)
        {
            Vector3 targetPos = (assignedChairSlot.platePoint != null)
                ? assignedChairSlot.platePoint.position
                : assignedChairSlot.position + new Vector3(-0.5f, 0f, 0f);

            plateObject.transform.position = targetPos;
            plateObject.transform.SetParent(null);
        }

        // Fica comendo por 5 segundos
        yield return new WaitForSeconds(5f);

        // Destrói apenas o pão de queijo
        if (plateObject != null)
        {
            Transform bread = plateObject.transform.Find("bread") ?? plateObject.transform.Find("Cheese") ?? plateObject.transform.GetChild(0);
            if (bread != null)
            {
                Destroy(bread.gameObject);
            }

            // REATIVA O COLISOR DO PRATO EXPLICITAMENTE:
            Collider2D plateCollider = plateObject.GetComponent<Collider2D>();
            if (plateCollider != null)
            {
                plateCollider.enabled = true;
            }
        }

        currentState = CustomerState.Leaving;
    }

    private void MoveTowards(Vector3 targetPosition, System.Action onArrival)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            if (targetPosition.x < transform.position.x) spriteRenderer.flipX = true;
            else if (targetPosition.x > transform.position.x) spriteRenderer.flipX = false;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            onArrival?.Invoke();
        }
    }

    public CustomerState GetCurrentState() => currentState;
    public TableManager.ChairSlot GetAssignedSeat() => assignedChairSlot;
}