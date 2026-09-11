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

    [Header("UI & Feedback (Filhos na Hierarquia)")]
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private GameObject speechBubble;

    [Header("Status Atual")]
    private CustomerState currentState;
    private TableManager.ChairSlot targetChairSlot;
    private int assignedChairIndex = -1;

    private Transform currentQueuePoint;
    private Vector3 exitPointPosition;
    private GameObject servedFood;
    private SpriteRenderer spriteRenderer;
    private Coroutine hideBubbleCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentState = CustomerState.Entering;

        // Garante que ambos começam desativados no Start
        if (exclamationMark != null) exclamationMark.SetActive(false);
        if (speechBubble != null) speechBubble.SetActive(false);
    }

    private void Update()
    {
        // Garante que o cliente fique visível em ordem positiva
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

                    // REGRA 1: Chegou no CounterPoint -> Liga a Exclamação
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

            case CustomerState.Leaving:
                MoveTowards(exitPointPosition);
                if (Vector2.Distance(transform.position, exitPointPosition) < 0.2f)
                {
                    if (assignedChairIndex != -1)
                    {
                        TableManager.Instance.ReleaseChair(assignedChairIndex);
                    }
                    Destroy(gameObject);
                }
                break;
        }
    }

    // REGRA 2: Interação com a tecla 'E'
    public void InteractWithCustomer()
    {
        // Se está esperando no balcão: Some a exclamação e Liga o Balão
        if (currentState == CustomerState.WaitingToOrder)
        {
            currentState = CustomerState.ShowingOrderBubble;

            if (exclamationMark != null) exclamationMark.SetActive(false);
            ShowSpeechBubble();

            // Inicia o timer de 5 segundos para sumir o balão sozinho e ir para a mesa
            hideBubbleCoroutine = StartCoroutine(WaitAndGoToTable(5f));
        }
        // Se interagir de novo ENQUANTO o balão está aberto: Some o balão imediatamente e vai para a mesa
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

            if (CustomerSpawner.Instance != null)
            {
                CustomerSpawner.Instance.NotifyCounterFreed();
            }
        }
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

    // Força o SpriteRenderer dos ícones a ficar visível e na frente do cliente
    private void ForceChildVisibility(GameObject childObj)
    {
        SpriteRenderer childSr = childObj.GetComponent<SpriteRenderer>();
        if (childSr != null && spriteRenderer != null)
        {
            childSr.enabled = true;
            childSr.sortingOrder = spriteRenderer.sortingOrder + 10; // Fica acima da cabeça do cliente
        }
    }

    public void TakeOrder() => InteractWithCustomer();
    public CustomerState GetCurrentState() => currentState;

    // Entrega de Comida
    public void ServeFood(GameObject food)
    {
        if (currentState != CustomerState.WaitingForFoodAtTable) return;

        bool hasBreadCheese = food.name.ToLower().Contains("bread") || food.name.ToLower().Contains("cheese");
        if (!hasBreadCheese)
        {
            foreach (Transform child in food.transform)
            {
                if (child.name.ToLower().Contains("bread") || child.name.ToLower().Contains("cheese"))
                {
                    hasBreadCheese = true;
                    break;
                }
            }
        }

        if (!hasBreadCheese) return;

        servedFood = food;

        if (targetChairSlot.platePoint != null)
        {
            servedFood.transform.SetParent(targetChairSlot.platePoint);
            servedFood.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        }

        currentState = CustomerState.Eating;
        StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(eatingTime);

        if (servedFood != null)
        {
            foreach (Transform child in servedFood.transform)
            {
                if (child.name.ToLower().Contains("bread") || child.name.ToLower().Contains("cheese"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        currentState = CustomerState.Leaving;
    }

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
}