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

        // Fechamento automático por distância
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > closeDistance)
            {
                CloseFridge();
                return;
            }
        }

        // Leitura direta de segurança para ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseFridge();
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
        // Aguarda o frame para o Canvas processar o SetActive
        yield return new WaitForEndOfFrame();

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