using System.Collections;
using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    public Vector3 spawnPos = new Vector3(9.42f, -0.03f, 0f);
    public Vector3 counterPos = new Vector3(2.82f, 0.02f, 0f);
    public float moveSpeed = 2.5f;

    public Sprite paoDeQueijoSprite;

    private bool isWaitingForFood = false;

    void Start()
    {
        transform.position = spawnPos;
        StartCoroutine(CustomerLogic());
    }

    private IEnumerator CustomerLogic()
    {
        // 1. Anda até o Balcão
        yield return MoveTo(counterPos);

        // 2. Anda até uma Cadeira Aleatória
        if (GameManager.Instance.chairs != null && GameManager.Instance.chairs.Length > 0)
        {
            int index = Random.Range(0, GameManager.Instance.chairs.Length);
            Transform targetChair = GameManager.Instance.chairs[index];
            if (targetChair != null)
            {
                yield return MoveTo(targetChair.position);
            }
        }

        // 3. Mostra Balão de Fala
        GameManager.Instance.ShowCustomerSpeech(paoDeQueijoSprite);
        isWaitingForFood = true;
    }

    public void ReceiveOrder()
    {
        if (isWaitingForFood)
        {
            isWaitingForFood = false;
            StartCoroutine(LeaveRoutine());
        }
    }

    private IEnumerator LeaveRoutine()
    {
        GameManager.Instance.HideCustomerSpeech();
        yield return MoveTo(spawnPos);
        GameManager.Instance.OnCustomerLeft();
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 dest)
    {
        while (Vector3.Distance(transform.position, dest) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}