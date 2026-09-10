using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Configurações da Borda")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private Vector2 outlineDistance = new Vector2(3f, -3f);

    private Outline outlineComponent;

    private void Awake()
    {
        // Garante que o componente Outline existe no mesmo objeto do botão
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }

        // Configura a cor e a espessura da borda
        outlineComponent.effectColor = outlineColor;
        outlineComponent.effectDistance = outlineDistance;

        // Inicia desativada para não aparecer nos botões que não estão focados
        outlineComponent.enabled = false;
    }

    // Chamado automaticamente pelo EventSystem quando o botão é SELECIONADO
    public void OnSelect(BaseEventData eventData)
    {
        if (outlineComponent != null)
        {
            outlineComponent.enabled = true;
        }
    }

    // Chamado automaticamente pelo EventSystem quando o botão PERDE O FOCO
    public void OnDeselect(BaseEventData eventData)
    {
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }

    private void OnDisable()
    {
        // Se a UI fechar, esconde a borda
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }
}