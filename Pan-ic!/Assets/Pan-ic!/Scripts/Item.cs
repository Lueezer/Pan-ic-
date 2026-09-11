using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Identificação do Item")]
    public string itemName = "Plate";

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;
    private SpriteRenderer mainSpriteRenderer;
    private Vector3 worldScale;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        mainSpriteRenderer = GetComponent<SpriteRenderer>();

        worldScale = transform.lossyScale;
    }

    public void OnPickUp(Transform holdPoint)
    {
        transform.SetParent(holdPoint);
        // Coloca o prato levemente pra frente da câmera em relação ao Player
        transform.localPosition = new Vector3(0f, 0f, -0.1f);

        if (holdPoint.lossyScale.x != 0 && holdPoint.lossyScale.y != 0)
        {
            transform.localScale = new Vector3(
                worldScale.x / holdPoint.lossyScale.x,
                worldScale.y / holdPoint.lossyScale.y,
                worldScale.z / holdPoint.lossyScale.z
            );
        }

        if (rigidbody2D != null) rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        if (collider2D != null) collider2D.enabled = false;

        UpdateLayersAndPositions(holdPoint);
    }

    public void OnDrop(Transform itemPoint)
    {
        transform.SetParent(itemPoint);
        // Coloca o prato levemente pra frente em relação à mesa
        transform.localPosition = new Vector3(0f, 0f, -0.1f);
        transform.localScale = worldScale;

        if (rigidbody2D != null) rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        if (collider2D != null) collider2D.enabled = true;

        UpdateLayersAndPositions(itemPoint);
    }

    private void UpdateLayersAndPositions(Transform parentTransform)
    {
        SpriteRenderer parentRenderer = parentTransform.GetComponentInParent<SpriteRenderer>();
        int baseOrder = (parentRenderer != null) ? parentRenderer.sortingOrder + 1 : 1;

        // 1. Configura o Prato
        if (mainSpriteRenderer != null)
        {
            mainSpriteRenderer.sortingOrder = baseOrder;
        }

        // 2. Configura os filhos (ex: Pão de Queijo) para NUNCA ficarem atrás
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in childRenderers)
        {
            if (sr != mainSpriteRenderer) // Se for uma comida/filho em cima do prato
            {
                sr.sortingOrder = baseOrder + 1; // Pão de queijo sempre +1 que o prato!
                sr.transform.localPosition = new Vector3(
                    sr.transform.localPosition.x,
                    sr.transform.localPosition.y,
                    -0.05f // Move pra frente do prato no eixo Z
                );
            }
        }
    }
}