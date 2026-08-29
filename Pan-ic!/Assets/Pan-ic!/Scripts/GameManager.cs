using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject fridgeUI;
    public GameObject customerSpeechBubble;
    public Image speechBubbleItemImage;

    [Header("Prefabs de Itens")]
    public GameObject polvilhoPrefab;
    public GameObject queijoPrefab;
    public GameObject bakingPanPrefab;
    public GameObject platePrefab;
    public GameObject cookedBreadPrefab;

    [Header("Configuração de Clientes")]
    public GameObject customerPrefab;
    public Transform[] chairs;
    public float spawnDelay = 5f;

    private bool hasActiveCustomer = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (fridgeUI != null) fridgeUI.SetActive(false);
        if (customerSpeechBubble != null) customerSpeechBubble.SetActive(false);
        StartCoroutine(SpawnCustomerRoutine());
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

    public void CloseFridge()
    {
        if (fridgeUI != null) fridgeUI.SetActive(false);
    }

    public void ShowCustomerSpeech(Sprite itemSprite)
    {
        if (customerSpeechBubble != null)
        {
            customerSpeechBubble.transform.position = new Vector3(4.2478f, 1.5113f, 0f);
            customerSpeechBubble.transform.localScale = new Vector3(1.767137f, 1.438383f, 1f);
            if (speechBubbleItemImage != null && itemSprite != null)
            {
                speechBubbleItemImage.sprite = itemSprite;
            }
            customerSpeechBubble.SetActive(true);
        }
    }

    public void HideCustomerSpeech()
    {
        if (customerSpeechBubble != null) customerSpeechBubble.SetActive(false);
    }

    public void OnCustomerLeft()
    {
        hasActiveCustomer = false;
        HideCustomerSpeech();
        StartCoroutine(SpawnCustomerRoutine());
    }

    private IEnumerator SpawnCustomerRoutine()
    {
        if (hasActiveCustomer) yield break;
        yield return new WaitForSeconds(spawnDelay);
        if (customerPrefab != null)
        {
            Instantiate(customerPrefab);
            hasActiveCustomer = true;
        }
    }
}