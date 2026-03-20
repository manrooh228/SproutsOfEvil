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
        public List<string> specialAbilities;

        [Header("Параметры Лоу-ХП")]
        private bool isEnraged = false; // Флаг, что эффект уже активен
        //private float damageMultiplier = 1.0f; // Текущий множитель урона

        [Header("Effects")]
        public GameObject textPrefab;
        public GameObject winMenuPrefab;
        public Transform headPoint;
        private Effects playerEf;
        private Effects myEf;

        void Start()
        {
            currentHealth = maxHealth;
            UpdateUI();
            playerEf = FindFirstObjectByType<Player>().GetComponent<Effects>();
            myEf = GetComponent<Effects>();
            // Инвертируем отталкивание, чтобы враг дергался в другую сторону
            myEf.knockbackDistance = myEf.knockbackDistance * -1;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            UpdateUI();

            // ПРОВЕРКА: Если HP упало ниже 50%, активируем IncreaseDamageLowHP
            CheckLowHPStatus();

            if (currentHealth <= 0) Die();
            myEf.PlayDamageEffect(damage);
        }

        private void CheckLowHPStatus()
        {
            // Проверяем 50% порог
            if (!isEnraged && currentHealth <= maxHealth * 0.5f)
            {
                isEnraged = true;
                // Сразу визуально оповещаем игрока
                SpawnAbilityText("ENRAGE: DMG UP!", Color.red);
            }
        }

        private void UpdateUI() => healthText.text = currentHealth.ToString();

        public IEnumerator ExecuteTurn()
        {
            int actionsCount = Random.Range(minActions, maxActions + 1);

            Debug.Log("ENEMY WANT TO DO " + actionsCount);
            for (int i = 0; i < actionsCount; i++)
            {
                yield return new WaitForSeconds(1.2f);
                PerformRandomAction();
            }
        }

        private void PerformRandomAction()
        {

            int choice = Random.Range(0, specialAbilities.Count + 1);

            if (choice == 0)
            {
                AttackPlayer();
            }
            else
            {
                string ability = specialAbilities[choice - 1];

                if (ability == "IncreaseDamageLowHP")
                {
                    PerformRandomAction();
                }
            }
        }

        private void AttackPlayer()
        {
            Player player = FindFirstObjectByType<Player>();

            // Рассчитываем урон: базовый + множитель ярости (например +50% урона если Enraged)
            float finalDamageMultiplier = isEnraged ? 1.5f : 1.0f;
            int damage = Mathf.RoundToInt(baseAttackDamage * finalDamageMultiplier);

            SpawnAbilityText(isEnraged ? "CRITICAL ATTACK!" : "ATTACK!", isEnraged ? Color.red : Color.yellow);

            playerEf.PlayDamageEffect(player.GetTotalUron(damage));
            player.TakeDamage(player.GetTotalUron(damage));
        }

        private void UseSpecialAbility(string abilityName)
        {
            //SpawnAbilityText(abilityName.ToUpper(), Color.cyan);

            //switch (abilityName)
            //{
                

            //        // Сюда можно добавить другие кейсы
            //}
        }

        void Die()
        {
            Destroy(gameObject);


            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;
            GameObject winMenu = Instantiate(winMenuPrefab, Vector2.zero, Quaternion.identity, canvasTransform);

        }

        private void SpawnAbilityText(string message, Color color)
        {
            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;
            GameObject t = Instantiate(textPrefab, headPoint.position, Quaternion.identity, canvasTransform);
            t.GetComponent<FloatingText>().SetText(message, color);
        }

        
    }
}