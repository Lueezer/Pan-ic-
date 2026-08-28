using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactionDistance = 1.5f;
    public Transform holdPoint; // Ponto onde o item fica preso nas mãos do jogador

    [Header("UI & Referências")]
    public GameObject fridgeUI;
    public GameObject heldItem;

    private Vector2 movement;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (fridgeUI != null) fridgeUI.SetActive(false);
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
        // Raio para identificar superfícies, geladeira, fogão, etc.
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionDistance);
        if (hit != null)
        {
            if (hit.CompareTag("Fridge"))
            {
                OpenFridge();
            }
        }
    }

    public void OpenFridge()
    {
        if (fridgeUI != null)
        {
            fridgeUI.transform.position = new Vector3(-4.4087f, 2.4422f, 0f);
            fridgeUI.transform.localScale = new Vector3(1.6027f, 2.095851f, 1f);
            fridgeUI.SetActive(true);
        }
    }

    public void PickUpItem(GameObject itemPrefab)
    {
        if (heldItem != null) return; // Já está segurando algo

        heldItem = Instantiate(itemPrefab, holdPoint.position, Quaternion.identity, holdPoint);

        // Garante que o prato/item fique na frente do personagem (Layer de renderização)
        SpriteRenderer sr = heldItem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 2; // Layer 2 pedida nas especificações
        }
    }
}