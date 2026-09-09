using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FridgeUI : MonoBehaviour
{
    [Header("UI da Geladeira")]
    [SerializeField] private GameObject fridgeUIPanel;
    [SerializeField] private Button firstSelectedButton;

    [Header("Configurações do Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float closeDistance = 2.5f;

    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;

    private bool isOpen = false;

    private void Start()
    {
        if (fridgeUIPanel != null)
            fridgeUIPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        // 1. Fechamento automático por distância
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > closeDistance)
            {
                CloseFridge();
                return;
            }
        }

        // 2. Disparo manual do botão focado via tecla E / Enter / Space
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TriggerSelectedButton();
            }

            // Tecla ESC para fechar
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseFridge();
            }
        }
    }

    private void TriggerSelectedButton()
    {
        GameObject selectedObj = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        if (selectedObj == null && firstSelectedButton != null)
        {
            selectedObj = firstSelectedButton.gameObject;
        }

        if (selectedObj != null)
        {
            Button currentBtn = selectedObj.GetComponent<Button>();
            if (currentBtn != null)
            {
                currentBtn.onClick.Invoke();
                Debug.Log($"[FridgeUI] Clique executado no botão: {selectedObj.name}");
            }
        }
    }

    public void ToggleFridge()
    {
        if (isOpen) CloseFridge();
        else OpenFridge();
    }

    public void OpenFridge()
    {
        isOpen = true;
        if (fridgeUIPanel != null)
        {
            fridgeUIPanel.SetActive(true);

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI");
            }

            StartCoroutine(SetFocusRoutine());
        }
    }

    private IEnumerator SetFocusRoutine()
    {
        yield return null; // Aguarda 1 frame para o Canvas processar o SetActive

        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            firstSelectedButton.Select();
        }
    }

    public void CloseFridge()
    {
        isOpen = false;
        if (fridgeUIPanel != null)
            fridgeUIPanel.SetActive(false);

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}