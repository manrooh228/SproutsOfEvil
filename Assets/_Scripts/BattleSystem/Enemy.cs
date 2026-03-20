using Assets._Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Состояние крестьянина")]
        private bool isEnraged = false; // Флаг, что эффект уже активен
        //private float damageMultiplier = 1.0f; // Текущий множитель урона

        [Header("Effects")]
        public GameObject textPrefab;
        public GameObject winMenuPrefab;
        public Transform headPoint;
        private Effects playerEf;
        private Effects myEf;

        [Header("Награды за победу")]
        public List<CardData> lootTable; // Сюда в инспекторе кидай карты, которые выпадут после смерти



        [Header("Состояние Рыцаря")]
        private bool isShielded = false;

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
            // Если щит активен, снижаем урон (например, на 50%)
            int finalDamage = isShielded ? Mathf.RoundToInt(damage * 0.5f) : damage;

            if (isShielded)
            {
                SpawnAbilityText("BLOCKED!", Color.blue);
                isShielded = false; // Щит ломается после получения урона или в начале следующего хода
            }

            currentHealth -= finalDamage;
            UpdateUI();
            StartCoroutine(HandleDamageAndEnrage(finalDamage));
        }

        private IEnumerator HandleDamageAndEnrage(int damage)
        {
            // 1. Сначала показываем эффект получения урона (тряска, вспышка)
            myEf.PlayDamageEffect(damage);

            // 2. Ждем, пока эффект урона почти закончится (например, 0.5 секунды)
            if (specialAbilities != null && specialAbilities.Contains("IncreaseDamageLowHP"))
            {
                yield return new WaitForSeconds(0.5f);
            }
            // 3. Проверяем: 
            // - Что эффект еще не активен (!isEnraged)
            // - Что враг еще жив (currentHealth > 0)
            // - Что здоровье упало ниже 50%
            // - ЧТО У ВРАГА ЕСТЬ НУЖНАЯ АБИЛКА (specialAbilities.Contains)
            if (!isEnraged && currentHealth > 0 && currentHealth <= maxHealth * 0.5f)
            {
                if (specialAbilities != null && specialAbilities.Contains("IncreaseDamageLowHP"))
                {
                    isEnraged = true;
                    SpawnAbilityText("ENRAGE: DMG UP!", Color.whiteSmoke);

                    // Небольшая пауза, чтобы игрок успел прочитать текст перед следующим событием
                    yield return new WaitForSeconds(0.3f);
                }
            }

            // 4. Только в самом конце проверяем смерть
            if (currentHealth <= 0)
            {
                Die();
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

                // Если это пассивка, перевыбираем действие
                if (ability == "IncreaseDamageLowHP")
                {
                    PerformRandomAction();
                }
                else
                {
                    UseSpecialAbility(ability);
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
            switch (abilityName)
            {
                case "SwordSmash":
                    SwordSmash();
                    break;
                case "ShieldCover":
                    ShieldCover();
                    break;
            }
        }

        //Knight
        private void SwordSmash()
        {
            Player player = FindFirstObjectByType<Player>();
            float enrageMult = isEnraged ? 1.5f : 1.0f;

            // Базовый урон * множитель ярости * 1.2 (бонус 20%)
            int damage = Mathf.RoundToInt(baseAttackDamage * enrageMult * 1.2f);

            SpawnAbilityText("SWORD SMASH!", Color.orangeRed);
            playerEf.PlayDamageEffect(player.GetTotalUron(damage));
            player.TakeDamage(player.GetTotalUron(damage));
        }
        private void ShieldCover()
        {
            isShielded = true;
            SpawnAbilityText("SHIELD COVER", Color.cyan);
        }

        void Die()
        {
            // 1. Делаем карты в руке невидимыми
            HidePlayerHand();

            // 2. Создаем меню победы
            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;
            GameObject winMenuObj = Instantiate(winMenuPrefab, canvasTransform);
            WinMenu winMenu = winMenuObj.GetComponent<WinMenu>();

            if (winMenu != null)
            {
                winMenu.SetupFixedRewards(lootTable);
            }

            // 3. Останавливаем логику руки, чтобы карты не добирались во время меню
            HandManager hand = FindFirstObjectByType<HandManager>();
            if (hand != null)
            {
                hand.StopAllCoroutines();
                // Отключаем скрипт, чтобы Update() и добор карт не работали
                hand.enabled = false;
            }

            Destroy(gameObject);

        }

        private void HidePlayerHand()
        {
            HandManager hand = FindFirstObjectByType<HandManager>();
            if (hand != null)
            {
                // Получаем доступ к списку объектов карт (метод в HandManager.cs должен быть public)
                List<GameObject> handCards = hand.GetCardsInHand();

                foreach (GameObject card in handCards)
                {
                    // Убираем видимость, отключая Image
                    Image cardImage = card.GetComponentInChildren<Image>();
                    if (cardImage != null)
                    {
                        cardImage.enabled = false;
                    }

                    // Опционально: отключаем текст, если он есть (TMPro)
                    CanvasGroup cardCanvasGroup = card.GetComponent<CanvasGroup>();
                    if (cardCanvasGroup != null)
                    {
                        cardCanvasGroup.alpha = 0f; // Делаем прозрачным всю группу (вместе с текстом)
                        cardCanvasGroup.blocksRaycasts = false; // Чтобы по ним нельзя было кликать
                    }
                }

                // Очищаем сам список, чтобы HandManager "забыл" о них (или оставь, если уничтожишь позже)
                // handCards.Clear();
            }
        }

        private void SpawnAbilityText(string message, Color color)
        { 
            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;
            GameObject t = Instantiate(textPrefab, headPoint.position, Quaternion.identity, canvasTransform);
            t.GetComponent<FloatingText>().SetText(message, color);
        }

        
    }
}