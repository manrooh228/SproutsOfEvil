using TMPro;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public int maxEnergy = 3;
    public int currentEnergy;
    public TextMeshProUGUI energyText;

    void Start() => ResetEnergy();

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    public bool CanAfford(int cost) => currentEnergy >= cost;

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
        UpdateUI();
    }

    private void UpdateUI() => energyText.text = $"{currentEnergy}/{maxEnergy}";
}
