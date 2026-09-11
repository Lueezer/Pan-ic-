using System.Collections;
using UnityEngine;

public enum CustomerState
{
    Entering,
    WaitingToOrder,
    ShowingOrderBubble,
    WalkingToTable,
    WaitingForFoodAtTable,
    Eating,
    Leaving
}

public class Customer : MonoBehaviour
{
    [Header("Configurações do Cliente")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float eatingTime = 5f;

    [Header("Detecção de Comida na Mesa")]
    [SerializeField] private float foodDetectionRadius = 0.6f; // Raio curto para pegar só o prato da frente dele
    [SerializeField] private LayerMask foodLayerMask;           // Pode deixar 'Default' ou criar layer de Itens
    [SerializeField] private float checkInterval = 0.5f;       // Checa a mesa a cada 0.5s

    [Header("UI & Feedback (Filhos na Hierarquia)")]
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private GameObject speechBubble;

    [Header("Status Atual")]
    private CustomerState currentState;
    private TableManager.ChairSlot targetChairSlot;
    private int assignedChairIndex = -1;

    private Transform currentQueuePoint;
    private Vector3 exitPointPosition;
    private SpriteRenderer spriteRenderer;
    private Coroutine hideBubbleCoroutine;
    private float foodCheckTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentState = CustomerState.Entering;

        if (exclamationMark != null) exclamationMark.SetActive(false);
        if (speechBubble != null) speechBubble.SetActive(false);
    }

    private void Update()
    {
        // Garante ordenação de profundidade visível na frente do chão/mesas
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.Max(1, Mathf.RoundToInt(-transform.position.y * 10) + 100);
        }

        switch (currentState)
        {
            case CustomerState.Entering:
                if (currentQueuePoint != null)
                {
                    MoveTowards(currentQueuePoint.position);

                    // Chegou no Balcão
                    if (Vector2.Distance(transform.position, currentQueuePoint.position) < 0.1f)
                    {
                        currentState = CustomerState.WaitingToOrder;
                        ShowExclamation();
                    }
                }
                break;

            case CustomerState.WalkingToTable:
                if (targetChairSlot.IsValid)
                {
                    MoveTowards(targetChairSlot.chairTransform.position);

                    if (Vector2.Distance(transform.position, targetChairSlot.chairTransform.position) < 0.1f)
                    {
                        currentState = CustomerState.WaitingForFoodAtTable;
                        print($"Cliente sentou na cadeira {assignedChairIndex} e aguarda a comida.");
                    }
                }
                break;

            case CustomerState.WaitingForFoodAtTable:
                // Checa periodicamente se colocaram a comida na mesa
                foodCheckTimer += Time.deltaTime;
                if (foodCheckTimer >= checkInterval)
                {
                    foodCheckTimer = 0f;
                    CheckTableForFood();
                }
                break;

            case CustomerState.Leaving:
                MoveTowards(exitPointPosition);
                if (Vector2.Distance(transform.position, exitPointPosition) < 0.2f)
                {
                    if (assignedChairIndex != -1 && TableManager.Instance != null)
                    {
                        TableManager.Instance.ReleaseChair(assignedChairIndex);
                    }
                    Destroy(gameObject);
                }
                break;
        }
    }

    // Interação no balcão com 'E'
    public void InteractWithCustomer()
    {
        if (currentState == CustomerState.WaitingToOrder)
        {
            currentState = CustomerState.ShowingOrderBubble;

            if (exclamationMark != null) exclamationMark.SetActive(false);
            ShowSpeechBubble();

            hideBubbleCoroutine = StartCoroutine(WaitAndGoToTable(5f));
        }
        else if (currentState == CustomerState.ShowingOrderBubble)
        {
            if (hideBubbleCoroutine != null) StopCoroutine(hideBubbleCoroutine);
            GoToRandomChair();
        }
    }

    private IEnumerator WaitAndGoToTable(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState == CustomerState.ShowingOrderBubble)
        {
            GoToRandomChair();
        }
    }

    private void GoToRandomChair()
    {
        if (speechBubble != null) speechBubble.SetActive(false);

        if (TableManager.Instance != null && TableManager.Instance.GetFreeChair(out TableManager.ChairSlot slot, out int index))
        {
            targetChairSlot = slot;
            assignedChairIndex = index;
            currentState = CustomerState.WalkingToTable;

            // MUDANÇA AQUI: Desativa a layer de interação do cliente no momento que ele sai do balcão.
            // O jogador não vai mais conseguir "colidir a interação" com ele enquanto ele anda ou fica sentado.
            gameObject.layer = LayerMask.NameToLayer("Default");

            if (CustomerSpawner.Instance != null)
            {
                CustomerSpawner.Instance.NotifyCounterFreed();
            }
        }
    }

    // --- LÓGICA DO RAIO NA MESA ---
    private void CheckTableForFood()
    {
        // Se a cadeira tem um ponto de prato (PlatePoint / ItemPoint na mesa)
        Vector3 checkCenter = targetChairSlot.platePoint != null ? targetChairSlot.platePoint.position : transform.position;

        // Raio pequeno para detectar o item em cima do ponto de refeição
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkCenter, foodDetectionRadius);

        foreach (Collider2D hit in hitColliders)
        {
            GameObject foundObject = hit.gameObject;

            // Busca pelo Pão de Queijo no próprio objeto ou nos filhos dele
            GameObject breadCheeseObj = FindBreadCheese(foundObject);

            if (breadCheeseObj != null)
            {
                print("Cliente encontrou o Pão de Queijo! Começando a comer...");
                currentState = CustomerState.Eating;
                StartCoroutine(EatRoutine(breadCheeseObj));
                break;
            }
        }
    }

    // Procura por Tag "Bread Cheese" ou nome do objeto contendo "bread" / "cheese"
    private GameObject FindBreadCheese(GameObject rootObj)
    {
        if (rootObj == null) return null;

        try
        {
            if (rootObj.CompareTag("Bread Cheese")) return rootObj;
        }
        catch { }

        string rootName = rootObj.name.ToLower();
        if (rootName.Contains("bread") || rootName.Contains("cheese") || rootName.Contains("pao"))
        {
            return rootObj;
        }

        foreach (Transform child in rootObj.transform)
        {
            try
            {
                if (child.CompareTag("Bread Cheese")) return child.gameObject;
            }
            catch { }

            string childName = child.name.ToLower();
            if (childName.Contains("bread") || childName.Contains("cheese") || childName.Contains("pao"))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private IEnumerator EatRoutine(GameObject breadCheeseObject)
    {
        // Aguarda os 5 segundos comendo
        yield return new WaitForSeconds(eatingTime);

        // Destrói apenas o Pão de Queijo (deixa o prato limpo na mesa)
        if (breadCheeseObject != null)
        {
            Destroy(breadCheeseObject);
            print("Cliente terminou de comer e destruiu o Pão de Queijo.");
        }

        // Vai embora
        currentState = CustomerState.Leaving;
    }

    private void ShowExclamation()
    {
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            ForceChildVisibility(exclamationMark);
        }
    }

    private void ShowSpeechBubble()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
            ForceChildVisibility(speechBubble);
        }
    }

    private void ForceChildVisibility(GameObject childObj)
    {
        SpriteRenderer childSr = childObj.GetComponent<SpriteRenderer>();
        if (childSr != null && spriteRenderer != null)
        {
            childSr.enabled = true;
            childSr.sortingOrder = spriteRenderer.sortingOrder + 10;
        }
    }

    public void TakeOrder() => InteractWithCustomer();
    public CustomerState GetCurrentState() => currentState;

    public void SetupCustomer(Transform queuePoint, Transform exitTransform)
    {
        currentQueuePoint = queuePoint;
        exitPointPosition = exitTransform != null ? exitTransform.position : transform.position;
        currentState = CustomerState.Entering;
    }

    public void SetupCustomer(Transform queuePoint, Vector3 exitVector)
    {
        currentQueuePoint = queuePoint;
        exitPointPosition = exitVector;
        currentState = CustomerState.Entering;
    }

    private void MoveTowards(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (targetChairSlot.platePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetChairSlot.platePoint.position, foodDetectionRadius);
        }
    }
}