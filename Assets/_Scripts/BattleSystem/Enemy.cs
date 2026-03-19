using Assets._Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.BattleSystem
{
    public class Enemy : MonoBehaviour
    {
        [Header("Статистика")]
        public int maxHealth;
        public int currentHealth;
        public TextMeshProUGUI healthText;

        [Header("Настройки Хода")]
        public int minActions = 1;
        public int maxActions = 2;

        [Header("Способности")]
        public int baseAttackDamage = 10;
        public List<string> specialAbilities; // Список названий или ID абилок

        [Header("Effects")]
        public GameObject textPrefab; // Тот же префаб текста
        public Transform headPoint;   // Точка над головой врага
        private Effects playerEf;
        private Effects myEf;

        void Start()
        {
            currentHealth = maxHealth;
            UpdateUI();
            playerEf = FindFirstObjectByType<Player>().GetComponent<Effects>();
            myEf = GetComponent<Effects>();
            myEf.knockbackDistance = myEf.knockbackDistance * -1;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            UpdateUI();
            if (currentHealth <= 0) Die();
            myEf.PlayDamageEffect(damage);
        }

        private void UpdateUI() => healthText.text = currentHealth.ToString();

        // Точка входа для начала хода врага
        public IEnumerator ExecuteTurn()
        {
            int actionsCount = Random.Range(minActions, maxActions + 1);
            Debug.Log($"{gameObject.name} планирует сделать {actionsCount} действий.");

            for (int i = 0; i < actionsCount; i++)
            {
                yield return new WaitForSeconds(1f); // Пауза между ударами
                PerformRandomAction();
            }
        }

        private void PerformRandomAction()
        {
            // Рандом: 0 - Атака, 1+ - СпешалАбилки
            int choice = Random.Range(0, specialAbilities.Count + 1);

            if (choice == 0)
            {
                AttackPlayer();
            }
            else
            {
                UseSpecialAbility(specialAbilities[choice - 1]);
            }
        }

        private void AttackPlayer()
        {
            SpawnAbilityText("АТАКА!", Color.yellow);
            playerEf.PlayDamageEffect(baseAttackDamage);
            // Здесь добавь ссылку на своего игрока:
            FindFirstObjectByType<Player>().TakeDamage(baseAttackDamage);
        }

        private void UseSpecialAbility(string abilityName)
        {
            SpawnAbilityText(abilityName.ToUpper(), Color.cyan);
            // Тут можно разветвить логику через switch(abilityName)
        }

        void Die() => Destroy(gameObject);

        private void SpawnAbilityText(string message, Color color)
        {
            // Найди Canvas на сцене (или создай переменную public Transform canvasTransform)
            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;

            // Спавни текст, передавая canvasTransform вторым аргументом
            GameObject t = Instantiate(textPrefab, headPoint.position, Quaternion.identity, canvasTransform);
            t.GetComponent<FloatingText>().SetText(message, color);
        }


    }
}