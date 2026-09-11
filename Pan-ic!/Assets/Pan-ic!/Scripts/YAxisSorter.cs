using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YAxisSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private int precision = 100;
    [SerializeField] private int baseOrder = 5000; // Valor base para evitar números negativos
    [SerializeField] private bool runOnlyAtStart = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateSorting();
    }

    void LateUpdate()
    {
        if (!runOnlyAtStart)
        {
            UpdateSorting();
        }
    }

    private void UpdateSorting()
    {
        // O baseOrder garante que a camada fique sempre acima do chão/fundo (que estão na camada 0)
        spriteRenderer.sortingOrder = baseOrder + (int)(-transform.position.y * precision);
    }
}