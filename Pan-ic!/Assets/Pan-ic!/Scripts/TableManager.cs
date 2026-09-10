using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance;

    [System.Serializable]
    public class ChairSlot
    {
        public int id;
        public Vector3 position;
        public bool isOccupied = false;

        public ChairSlot(int id, Vector3 position)
        {
            this.id = id;
            this.position = position;
            this.isOccupied = false;
        }
    }

    [Header("Lista de Lugares das Cadeiras")]
    public List<ChairSlot> availableChairs = new List<ChairSlot>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicializa as 4 cadeiras com as coordenadas fixadas
        InitializeChairs();
    }

    private void InitializeChairs()
    {
        availableChairs.Clear();

        // Coordenadas exatas das 4 cadeiras
        availableChairs.Add(new ChairSlot(1, new Vector3(4.11f, 2.97f, 0f)));
        availableChairs.Add(new ChairSlot(2, new Vector3(7.57f, 2.97f, 0f)));
        availableChairs.Add(new ChairSlot(3, new Vector3(4.20f, -3.45f, 0f)));
        availableChairs.Add(new ChairSlot(4, new Vector3(7.63f, -3.45f, 0f)));
    }

    /// <summary>
    /// Verifica se existe pelo menos uma cadeira vaga.
    /// </summary>
    public bool HasFreeChair()
    {
        foreach (var slot in availableChairs)
        {
            if (!slot.isOccupied) return true;
        }
        return false;
    }

    /// <summary>
    /// Busca uma cadeira livre aleatória e marca como ocupada.
    /// </summary>
    public ChairSlot GetRandomFreeChair()
    {
        List<ChairSlot> freeChairs = new List<ChairSlot>();

        foreach (var slot in availableChairs)
        {
            if (!slot.isOccupied)
            {
                freeChairs.Add(slot);
            }
        }

        if (freeChairs.Count > 0)
        {
            int randomIndex = Random.Range(0, freeChairs.Count);
            freeChairs[randomIndex].isOccupied = true;
            Debug.Log($"[TableManager] Cliente reservou a Cadeira {freeChairs[randomIndex].id}");
            return freeChairs[randomIndex];
        }

        Debug.LogWarning("[TableManager] Nenhuma cadeira livre encontrada!");
        return null;
    }

    /// <summary>
    /// Libera a cadeira para o próximo cliente.
    /// </summary>
    public void ReleaseChair(ChairSlot slot)
    {
        if (slot != null)
        {
            slot.isOccupied = false;
            Debug.Log($"[TableManager] Cadeira {slot.id} liberada.");
        }
    }
}