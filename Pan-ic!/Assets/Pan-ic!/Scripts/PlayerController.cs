using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Pontos de Interação")]
    public Transform holdPoint;

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

        print("1. Botão Interagir Pressionado!");

        // Busca todos os colisores no raio do HoldPoint
        Collider2D[] hits = Physics2D.OverlapCircleAll(holdPoint.position, interactRadius, interactableLayer);

        FridgeUI fridgeFound = null;
        Item itemFound = null;

        foreach (Collider2D hit in hits)
        {
            // Ignora o próprio jogador
            if (hit.gameObject == gameObject) continue;

            // 1. Procura por uma Geladeira no alcance
            FridgeUI fridge = hit.GetComponent<FridgeUI>();
            if (fridge != null)
            {
                fridgeFound = fridge;
                break;
            }

            // 2. Procura por um Item solto no alcance
            Item item = hit.GetComponent<Item>();
            if (item != null)
            {
                itemFound = item;
                break;
            }
        }

        // Se encontrou a geladeira, ela SEMPRE abre/fecha a janela (mesmo com a mão cheia)
        if (fridgeFound != null)
        {
            print("2. Geladeira encontrada! Alternando janela da interface...");
            fridgeFound.ToggleFridge();
            return;
        }

        // Se não interagiu com a geladeira, segue a lógica normal de pegar ou soltar
        if (currentItem == null)
        {
            if (itemFound != null)
            {
                print($"3. Item '{itemFound.itemName}' encontrado! Pegando item...");
                currentItem = itemFound;
                currentItem.OnPickUp(holdPoint);
            }
            else
            {
                print("ALERTA: Nenhum objeto interativo no alcance!");
            }
        }
        else
        {
            print("4. Soltando item...");
            currentItem.OnDrop(holdPoint.position);
            currentItem = null;
        }
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
    }

    // ==========================================
    // Métodos auxiliares para a UI da Geladeira
    // ==========================================

    /// <summary>
    /// Retorna 'true' se o jogador já estiver segurando algo na mão.
    /// </summary>
    public bool HasItemInHand()
    {
        return currentItem != null;
    }

    /// <summary>
    /// Coloca um item (gerado pelos botões da geladeira) diretamente na mão do jogador.
    /// </summary>
    public void GiveItemToHand(Item newItem)
    {
        if (newItem == null) return;

        currentItem = newItem;
        currentItem.OnPickUp(holdPoint);
    }
}