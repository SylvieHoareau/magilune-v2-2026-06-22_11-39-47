using UnityEngine;
using UnityEngine.UI;

public class JetpackFuelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fuelBar;           // → FuelBar (Filled)
    [SerializeField] private GameObject fuelContainer; // le parent JetpackFuelUI
    [SerializeField] private PlayerJetpack jetpack;

    [Header("Optional")]
    [SerializeField] private Image jetpackIcon;       // pour animer la flamme plus tard

    private void Update()
    {
        if (jetpack == null) return;

        // Cache toute la jauge tant que le jetpack n'est pas déverrouillé
        if (fuelContainer != null)
            fuelContainer.SetActive(jetpack.IsUnlocked);

        if (!jetpack.IsUnlocked || fuelBar == null) return;

        // Animation fluide
        fuelBar.fillAmount = Mathf.Lerp(
            fuelBar.fillAmount,
            jetpack.FuelNormalized,
            Time.deltaTime * 10f
        );
    }
}
