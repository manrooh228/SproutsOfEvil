using Assets._Scripts.BattleSystem;
using Assets._Scripts.UI;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class ActiveStatus
{
    public StatusEffectType type;
    public int value;
    public int remainingTurns;
}

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

    [Header("Statuses")]
    private List<ActiveStatus> activeStatuses = new List<ActiveStatus>();
    private int strengthBonus = 0; // Бонус к урону
    private float agilityModifier = 1f; // Множитель входящего урона (ловкость)


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
        int finalDamage = GetTotalUron(damage);

        if (armor > 0)
        {
            if (finalDamage <= armor)
            {
                armor -= finalDamage;
                finalDamage = 0;
            }
            else
            {
                finalDamage -= armor;
                armor = 0;
            }
        }

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI(); 

        if (currentHealth <= 0) Die();

    }

    public int GetTotalDamage(int baseDamage) => baseDamage + strengthBonus;
    public int GetTotalUron(int baseDamage) => Mathf.RoundToInt(baseDamage * agilityModifier);

    public void Heal(int amount)
    {
        currentHealth += amount;

        // Ограничиваем хилл максимальным запасом здоровья
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateUI();

        // Вызываем визуальный эффект текста (зеленый цвет)
        if (playerEffects != null)
        {
            playerEffects.PlayAbilityText($"+{amount} HP", Color.green);
        }
    }

    private void UpdateUI()
    {
        if (armor > 0)
            healthText.text = $"{currentHealth}+{armor}";
        else
            healthText.text = currentHealth.ToString();
    }
    void Die() => Debug.Log("ты проиграл!");



    //Statuses

    public void ApplyStatus(StatusEffectType type, int value, int duration)
    {
        // Ищем, есть ли уже такой эффект у игрока
        ActiveStatus existingStatus = activeStatuses.Find(s => s.type == type);

        if (existingStatus != null)
        {
            // Если эффект есть — суммируем длительность
            existingStatus.remainingTurns += duration;

            // Опционально: можно также обновлять значение (value), 
            // если ты хочешь, чтобы сила эффекта тоже росла
            // existingStatus.value += value; 
            if (playerEffects != null)
            {
                playerEffects.PlayAbilityText($"The effect has been extended. {existingStatus.remainingTurns} turn left", Color.white);
            }
        }
        else
        {
            // Если эффекта нет — создаем новый
            activeStatuses.Add(new ActiveStatus
            {
                type = type,
                value = value,
                remainingTurns = duration
            });
            if (playerEffects != null)
            {
                playerEffects.PlayAbilityText($"Added a new {type} effect to the {duration} of moves", Color.white);
            }
        }

        UpdateUI();
    }
    public void ProcessStatuses()
    {
        // Сбрасываем временные модификаторы перед пересчетом
        strengthBonus = 0;
        agilityModifier = 1f;

        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            var status = activeStatuses[i];

            switch (status.type)
            {
                case StatusEffectType.DamageOverTime:
                    // Наносим урон врагу (нужна ссылка на врага)
                    FindFirstObjectByType<Enemy>()?.TakeDamage(status.value);
                    Debug.Log(status.type + status.remainingTurns + " left");
                    break;
                case StatusEffectType.HealOverTime:
                    Heal(status.value);
                    Debug.Log(status.type + status.remainingTurns + " left");
                    break;
                case StatusEffectType.BlockOverTime:
                    AddArmor(status.value);
                    Debug.Log(status.type + status.remainingTurns + " left");
                    break;
                case StatusEffectType.StrengthBuff:
                    strengthBonus += status.value;
                    Debug.Log(status.type + status.remainingTurns + " left");
                    break;
                case StatusEffectType.AgilityBuff:
                    // Уменьшаем входящий урон на % (например, value = 50 значит 50% урона)
                    agilityModifier = (100 - status.value) / 100f;
                    Debug.Log(status.type.ToString() + status.remainingTurns + " left");
                    break;//чет не работает
            }

            status.remainingTurns--;
            if (status.remainingTurns <= 0) activeStatuses.RemoveAt(i);
        }
        UpdateUI();
    }
}
