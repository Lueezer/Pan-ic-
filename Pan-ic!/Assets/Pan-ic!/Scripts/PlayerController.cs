using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Pontos de Interação / Raio")]
    public Transform holdPoint;          // Mão Direita (onde o item segura)
    public Transform leftInteractPoint;  // Mão Esquerda (só para detectar a mesa/balcão do outro lado)

    [Header("Configurações de Interação")]
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Item currentItem;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // Busca acertos no lado direito e no lado esquerdo
        Collider2D[] hitsRight = Physics2D.OverlapCircleAll(holdPoint.position, interactRadius, interactableLayer);

        Vector3 leftPos = (leftInteractPoint != null) ? leftInteractPoint.position : holdPoint.position;
        Collider2D[] hitsLeft = Physics2D.OverlapCircleAll(leftPos, interactRadius, interactableLayer);

        // Junta os dois arrays de acertos
        Collider2D[] hits = CombineColliders(hitsRight, hitsLeft);

        FridgeUI fridgeFound = null;
        Customer customerFound = null;
        Item itemFound = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            FridgeUI fridge = hit.GetComponent<FridgeUI>();
            if (fridge != null) { fridgeFound = fridge; break; }

            Customer customer = hit.GetComponent<Customer>();
            if (customer != null) { customerFound = customer; break; }

            Item item = hit.GetComponent<Item>();
            if (item != null) { itemFound = item; break; }
        }

        // 1. Interação Geladeira
        if (fridgeFound != null)
        {
            fridgeFound.ToggleFridge();
            return;
        }

        // 2. Interação Cliente
        if (customerFound != null)
        {
            if (customerFound.GetCurrentState() == CustomerState.WaitingToOrder)
            {
                customerFound.TakeOrder();
                return;
            }
            else if (customerFound.GetCurrentState() == CustomerState.WaitingForFoodAtTable && currentItem != null)
            {
                Item plateToDeliver = currentItem;
                currentItem = null;
                plateToDeliver.transform.SetParent(null);
                customerFound.ServeFood(plateToDeliver.gameObject);
                return;
            }
        }

        // 3. Pegar / Soltar Item
        if (currentItem == null)
        {
            if (itemFound != null)
            {
                currentItem = itemFound;
                // SEMPRE pega na mão direita (holdPoint)
                currentItem.OnPickUp(holdPoint);
            }
        }
        else
        {
            // Procura o ItemPoint mais próximo (seja do lado esquerdo ou direito)
            Transform targetItemPoint = FindClosestFreeItemPoint();

            if (targetItemPoint != null)
            {
                currentItem.OnDrop(targetItemPoint);
                currentItem = null;
            }
        }
    }

    private Transform FindClosestFreeItemPoint()
    {
        Vector3 leftPos = (leftInteractPoint != null) ? leftInteractPoint.position : holdPoint.position;

        Collider2D[] hitsRight = Physics2D.OverlapCircleAll(holdPoint.position, interactRadius, interactableLayer);
        Collider2D[] hitsLeft = Physics2D.OverlapCircleAll(leftPos, interactRadius, interactableLayer);

        Collider2D[] allHits = CombineColliders(hitsRight, hitsLeft);

        Transform closestPoint = null;
        float minDistance = float.MaxValue;

        foreach (var hit in allHits)
        {
            if (hit.CompareTag("ItemPoint"))
            {
                if (hit.transform.childCount == 0) // Ponto livre sem filhos
                {
                    // Checa a distância tanto da mão direita quanto da esquerda
                    float distRight = Vector2.Distance(holdPoint.position, hit.transform.position);
                    float distLeft = Vector2.Distance(leftPos, hit.transform.position);
                    float shortestDist = Mathf.Min(distRight, distLeft);

                    if (shortestDist < minDistance)
                    {
                        minDistance = shortestDist;
                        closestPoint = hit.transform;
                    }
                }
            }
        }

        return closestPoint;
    }

    private Collider2D[] CombineColliders(Collider2D[] a, Collider2D[] b)
    {
        Collider2D[] result = new Collider2D[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rigidbody2D.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        Camera mainCam = Camera.main;
        Vector3 minBounds = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxBounds = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float spriteHalfWidth = spriteRenderer.bounds.extents.x;
        float spriteHalfHeight = spriteRenderer.bounds.extents.y;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x + spriteHalfWidth, maxBounds.x - spriteHalfWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y + spriteHalfHeight, maxBounds.y - spriteHalfHeight);

        rigidbody2D.MovePosition(targetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (holdPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(holdPoint.position, interactRadius);
        }
        if (leftInteractPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftInteractPoint.position, interactRadius);
        }
    }

    public bool HasItemInHand() => currentItem != null;

    public void GiveItemToHand(Item newItem)
    {
        if (newItem == null) return;
        currentItem = newItem;
        currentItem.OnPickUp(holdPoint);
    }
}