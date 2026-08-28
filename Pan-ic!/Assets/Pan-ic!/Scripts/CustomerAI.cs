using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerAI : MonoBehaviour
{
    [Header("Posições")]
    public Vector3 spawnPos = new Vector3(9.42f, -0.03f, 0f);
    public Vector3 counterPos = new Vector3(2.82f, 0.02f, 0f);
    public Transform[] chairs; // Array com as 6 cadeiras da cena

    [Header("UI de Balão de Fala")]
    public GameObject speechBubble;
    public Image paitingImage;

    public float moveSpeed = 2.5f;
    private Transform targetChair;

    void Start()
    {
        transform.position = spawnPos;
        StartCoroutine(CustomerFlow());
    }

    private IEnumerator CustomerFlow()
    {
        // 1. Anda até o balcão
        yield return MoveTo(counterPos);

        // 2. Escolhe uma cadeira aleatória (1 a 6)
        if (chairs != null && chairs.Length > 0)
        {
            int randomIndex = Random.Range(0, chairs.Length);
            targetChair = chairs[randomIndex];
            yield return MoveTo(targetChair.position);
        }

        // 3. Mostra o balão de fala com o pão de queijo
        ShowSpeechBubble();

        // 4. Aguarda ser servido (Simulação de espera)
        yield return new WaitForSeconds(8f);

        // 5. Esconde o balão e vai embora
        if (speechBubble != null) speechBubble.SetActive(false);
        yield return MoveTo(spawnPos);

        // Notifica o Spawner e é destruído
        CustomerSpawner.instance.OnCustomerLeft();
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void ShowSpeechBubble()
    {
        if (speechBubble != null)
        {
            speechBubble.transform.position = new Vector3(4.2478f, 1.5113f, 0f);
            speechBubble.transform.localScale = new Vector3(1.767137f, 1.438383f, 1f);
            speechBubble.SetActive(true);
        }
    }
}