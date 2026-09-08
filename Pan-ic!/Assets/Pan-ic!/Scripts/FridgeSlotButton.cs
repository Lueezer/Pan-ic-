using UnityEngine;

public class FridgeSlotButton : MonoBehaviour
{
    [Header("Configurações do Ingrediente")]
    [Tooltip("Arraste para cá o Prefab do ingrediente (ex: Queijo ou Ovo)")]
    [SerializeField] private GameObject ingredientPrefab;

    [Header("Referência ao Player")]
    [Tooltip("Arraste o Player da Hierarchy para cá")]
    [SerializeField] private PlayerController player;

    public void OnClickSelectIngredient()
    {
        Debug.Log("--- BOTÃO DA GELADEIRA FOI CLICADO COM SUCESSO! ---");

        if (player == null || ingredientPrefab == null)
        {
            Debug.LogWarning("[FridgeSlotButton] Falta atribuir o Player ou o Prefab do Ingrediente no Inspector!");
            return;
        }

        // Se a mão do jogador estiver VAZIA, gera e entrega o item
        if (!player.HasItemInHand())
        {
            GameObject newItemObj = Instantiate(ingredientPrefab);
            Item itemScript = newItemObj.GetComponent<Item>();

            if (itemScript != null)
            {
                player.GiveItemToHand(itemScript);
                Debug.Log($"[UI Geladeira] {itemScript.itemName} entregue para o jogador!");
            }
        }
        else
        {
            Debug.Log("[UI Geladeira] Mão ocupada! Não é possível pegar o ingrediente agora.");
        }
    }
}