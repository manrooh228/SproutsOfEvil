using Assets._Scripts.UI;
using System.Net;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;
    public int armor = 0;

    public TextMeshProUGUI healthText;
    private Effects playerEffects;

    [Header("Effects")]
    public GameObject textPrefab; // Тот же префаб текста
    public Transform headPoint;

    private void Awake()
    {
        playerEffects = GetComponent<Effects>();
    }
    public void AddArmor(int amount)
    {
        armor += amount;
        UpdateUI();
        // Можно добавить отдельный эффект появления брони (например, синий текст)
        if (playerEffects != null) playerEffects.PlayAbilityText($"+{amount} BLOCK", Color.blue);
    }

    private void Start()
    {
        UpdateUI();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)

    {
        if (armor > 0)
        {
            if (damage <= armor)
            {
                armor -= damage;
                damage = 0;
            }
            else
            {
                damage -= armor;
                armor = 0;
            }
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0) Die();

    }

    private void UpdateUI()
    {
        if (armor > 0)
            healthText.text = $"{currentHealth}+{armor}";
        else
            healthText.text = currentHealth.ToString();
    }
    void Die() => Debug.Log("ты проиграл!");

}
