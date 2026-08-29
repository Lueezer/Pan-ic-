using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactionDistance = 1.5f;
    public Transform holdPoint;
    public GameObject currentHeldItem;
    public string heldItemType = ""; // "", "Polvilho", "Queijo", "FormaVazia", "FormaComMassa", "PaoQueijo", "Prato", "PratoComPao"

    private Vector2 movement;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionDistance);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Fridge"))
            {
                GameManager.Instance.OpenFridge();
                return;
            }
            if (hit.CompareTag("Stove"))
            {
                StoveTile stove = hit.GetComponent<StoveTile>();
                if (stove != null) stove.Interact(this);
                return;
            }
            if (hit.CompareTag("Customer"))
            {
                CustomerAI customer = hit.GetComponent<CustomerAI>();
                if (customer != null && heldItemType == "PratoComPao")
                {
                    customer.ReceiveOrder();
                    ClearHeldItem();
                }
                return;
            }
        }
    }

    public void PickItem(GameObject prefab, string itemType)
    {
        if (currentHeldItem != null) Destroy(currentHeldItem);

        heldItemType = itemType;
        currentHeldItem = Instantiate(prefab, holdPoint.position, Quaternion.identity, holdPoint);

        SpriteRenderer sr = currentHeldItem.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 2; // Coloca na Layer 2 na frente do player
    }

    public void ClearHeldItem()
    {
        if (currentHeldItem != null) Destroy(currentHeldItem);
        heldItemType = "";
    }

    // Botões da UI da Geladeira chamam essas funções:
    public void GetPolvilhoFromFridge()
    {
        PickItem(GameManager.Instance.polvilhoPrefab, "Polvilho");
        GameManager.Instance.CloseFridge();
    }

    public void GetQueijoFromFridge()
    {
        PickItem(GameManager.Instance.queijoPrefab, "Queijo");
        GameManager.Instance.CloseFridge();
    }
}