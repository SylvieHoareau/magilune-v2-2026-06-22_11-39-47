using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance { get; private set; }

    public int CurrentCrystals { get; private set; }

    public event System.Action<int> OnCrystalChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCrystal(int amount = 1)
    {
        if (amount <= 0) return;
        CurrentCrystals += amount;
        Debug.Log($"Cristaux = {CurrentCrystals}");  // ← à voir dans la Console
        OnCrystalChanged?.Invoke(CurrentCrystals);
    }

    public void ResetCrystals()
    {
        CurrentCrystals = 0;
        OnCrystalChanged?.Invoke(CurrentCrystals);
    }
}
