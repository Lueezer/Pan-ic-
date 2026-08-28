using System.Collections;
using UnityEngine;
using TMPro;

public class CookingSystem : MonoBehaviour
{
    [Header("Ingredientes & Estados")]
    public bool hasPolvilho = false;
    public bool hasQueijo = false;
    public bool isBaking = false;
    public bool isReady = false;

    [Header("UI Timer")]
    public TextMeshProUGUI timerText;
    public float cookTime = 10f;

    public void AddIngredient(string ingredient)
    {
        if (ingredient == "Polvilho") hasPolvilho = true;
        if (ingredient == "Queijo") hasQueijo = true;

        if (hasPolvilho && hasQueijo)
        {
            Debug.Log("Massa de Pão de Queijo pronta na forma!");
        }
    }

    public void PlaceOnOven(Collider2D surfaceCollider)
    {
        // Validação estrita: Não pode ser colocado no chão, paredes ou geladeira
        if (!surfaceCollider.CompareTag("Countertop"))
        {
            Debug.LogWarning("Item só pode ser colocado nas bancadas válidas!");
            return;
        }

        if (hasPolvilho && hasQueijo && !isBaking && !isReady)
        {
            StartCoroutine(StartBakingRoutine());
        }
    }

    private IEnumerator StartBakingRoutine()
    {
        isBaking = true;
        float remainingTime = cookTime;

        while (remainingTime > 0)
        {
            if (timerText != null)
            {
                timerText.text = $"Assando: {remainingTime:F1}s";
            }
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }

        isBaking = false;
        isReady = true;
        if (timerText != null) timerText.text = "Pão de Queijo Pronto!";
    }
}