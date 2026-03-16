using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;

    public TextMeshProUGUI healthText;

    private void Start()
    {
        UpdateUI();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)

    {
        currentHealth -= damage;

        UpdateUI();

        if (currentHealth <= 0) Die();

    }

    private void UpdateUI() => healthText.text = currentHealth.ToString();

    void Die() => Debug.Log("ты проиграл!");
}
