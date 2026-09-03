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

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

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

    private void FixedUpdate()
    {
        // 1. Calcula a posição para onde o player quer ir
        Vector2 targetPosition = rigidbody2D.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // 2. Limites da Câmera
        Camera mainCam = Camera.main;
        Vector3 minBounds = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxBounds = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // 3. Metade do tamanho do Sprite
        float spriteHalfWidth = spriteRenderer.bounds.extents.x;
        float spriteHalfHeight = spriteRenderer.bounds.extents.y;

        // 4. Aplica o Clamp na posição final antes de mover a física
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x + spriteHalfWidth, maxBounds.x - spriteHalfWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y + spriteHalfHeight, maxBounds.y - spriteHalfHeight);

        // 5. Move a física já com a posição travada
        rigidbody2D.MovePosition(targetPosition);
    }
}