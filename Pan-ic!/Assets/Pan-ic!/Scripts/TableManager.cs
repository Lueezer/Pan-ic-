using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    [System.Serializable]
    public struct ChairSlot
    {
        public int id;
        public Transform chairTransform;
        public Transform platePoint;
        public bool isOccupied;

        public Vector3 position => chairTransform != null ? chairTransform.position : Vector3.zero;
        public bool IsValid => chairTransform != null;
    }

    [Header("Configuração das Cadeiras")]
    public List<ChairSlot> availableChairs = new List<ChairSlot>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool GetFreeChair(out ChairSlot freeSlot, out int index)
    {
        List<int> freeIndices = new List<int>();

        for (int i = 0; i < availableChairs.Count; i++)
        {
            if (!availableChairs[i].isOccupied && availableChairs[i].chairTransform != null)
            {
                freeIndices.Add(i);
            }
        }

        if (freeIndices.Count > 0)
        {
            // Seleciona um índice aleatório das cadeiras disponíveis
            int randomIndex = freeIndices[Random.Range(0, freeIndices.Count)];
            index = randomIndex;

            ChairSlot updatedSlot = availableChairs[randomIndex];
            updatedSlot.isOccupied = true;
            availableChairs[randomIndex] = updatedSlot;

            freeSlot = availableChairs[randomIndex];
            return true;
        }

        freeSlot = default;
        index = -1;
        return false;
    }

    public void ReleaseChair(int index)
    {
        if (index >= 0 && index < availableChairs.Count)
        {
            ChairSlot updatedSlot = availableChairs[index];
            updatedSlot.isOccupied = false;
            availableChairs[index] = updatedSlot;
        }
    }

    public bool HasFreeChair() => HasAvailableChair();

    public bool HasAvailableChair()
    {
        foreach (var slot in availableChairs)
        {
            if (!slot.isOccupied && slot.chairTransform != null)
                return true;
        }
        return false;
    }
}