using System.Collections;
using UnityEngine;
using TMPro;

public class StoveTile : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float cookTime = 10f;

    private bool hasPolvilho = false;
    private bool hasQueijo = false;
    private bool isCooking = false;
    private bool isReady = false;

    public void Interact(PlayerController player)
    {
        if (isCooking) return;

        if (isReady && player.heldItemType == "Prato")
        {
            // Transfere o Pão de Queijo para o Prato
            player.PickItem(GameManager.Instance.cookedBreadPrefab, "PratoComPao");
            isReady = false;
            hasPolvilho = false;
            hasQueijo = false;
            if (timerText != null) timerText.text = "";
            return;
        }

        if (player.heldItemType == "Polvilho")
        {
            hasPolvilho = true;
            player.ClearHeldItem();
            CheckIngredients();
        }
        else if (player.heldItemType == "Queijo")
        {
            hasQueijo = true;
            player.ClearHeldItem();
            CheckIngredients();
        }
    }

    private void CheckIngredients()
    {
        if (hasPolvilho && hasQueijo && !isCooking && !isReady)
        {
            StartCoroutine(StartCooking());
        }
    }

    private IEnumerator StartCooking()
    {
        isCooking = true;
        float remaining = cookTime;

        while (remaining > 0)
        {
            if (timerText != null) timerText.text = $"{remaining:F1}s";
            yield return new WaitForSeconds(0.1f);
            remaining -= 0.1f;
        }

        isCooking = false;
        isReady = true;
        if (timerText != null) timerText.text = "Pão de Queijo Pronto!";
    }
}