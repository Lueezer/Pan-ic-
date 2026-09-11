using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance;

    [System.Serializable]
    public class ChairSlot
    {
        public int id;
        public Vector3 position;       // Posição da Cadeira
        public Transform platePoint;   // Ponto onde o prato fica na mesa (opcional)
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

        // Inicializa as 4 cadeiras automaticamente com as coordenadas exatas
        InitializeChairs();
    }

    private void InitializeChairs()
    {
        availableChairs.Clear();

        // Coordenadas exatas passadas por você:
        availableChairs.Add(new ChairSlot(1, new Vector3(4.11f, 2.97f, 0f)));
        availableChairs.Add(new ChairSlot(2, new Vector3(7.57f, 2.97f, 0f)));
        availableChairs.Add(new ChairSlot(3, new Vector3(4.20f, -3.45f, 0f)));
        availableChairs.Add(new ChairSlot(4, new Vector3(7.63f, -3.45f, 0f)));
    }

    public bool HasFreeChair()
    {
        foreach (var slot in availableChairs)
        {
            if (!slot.isOccupied) return true;
        }
        return false;
    }

    public ChairSlot GetRandomFreeChair()
    {
        List<ChairSlot> freeChairs = new List<ChairSlot>();

        foreach (var slot in availableChairs)
        {
            if (!slot.isOccupied) freeChairs.Add(slot);
        }

        if (freeChairs.Count > 0)
        {
            int randomIndex = Random.Range(0, freeChairs.Count);
            freeChairs[randomIndex].isOccupied = true;
            return freeChairs[randomIndex];
        }

        return null;
    }

    public void ReleaseChair(ChairSlot slot)
    {
        if (slot != null)
        {
            slot.isOccupied = false;
        }
    }
}