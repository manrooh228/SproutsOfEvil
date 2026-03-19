using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.UI
{
    public class Effects : MonoBehaviour
    {
        [Header("Ссылки")]
        public SpriteRenderer spriteRenderer;
        public GameObject textPrefab;
        public Transform textSpawnPoint;

        [Header("Настройки Беления (Shader)")]
        // Ссылка на материал, который мы создали (со встроенным Shader Graph)
        public Material flashMaterial;
        public Color flashColor = Color.white;
        public float flashDuration = 0.3f; // Сколько держится белый цвет

        [Header("Настройки Отталкивания (Knockback)")]
        public float knockbackDistance = 0.1f; // На сколько пикселей/юнитов назад
        public float knockbackSpeed = 3f;    // Скорость отхода назад
        public float returnSpeed = 1f;       // Скорость возвращения

        private Material originalMaterial;
        // private int flashAmountID; // Задел на будущее, если анимировать через код

        void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // Сохраняем оригинальный материал (скорее всего Sprite-Lit или Unlit)
            originalMaterial = spriteRenderer.material;

            // ID для шейдера, если захотим анимировать плавно (в этом примере просто подменяем материал)
            // flashAmountID = Shader.PropertyToID("_FlashAmount");
        }

        public void PlayDamageEffect(int damage)
        {
            // 1. Текст урона
            SpawnFloatingText($"-{damage}", Color.red);

            // 2. Визуальный отклик (Цвет + Движение)
            StopAllCoroutines(); // Чтобы эффекты не накладывались
            StartCoroutine(FlashAndKnockbackRoutine());
        }

        public void PlayAbilityText(string text, Color color)
        {
            SpawnFloatingText(text, color);
        }

        private void SpawnFloatingText(string text, Color color)
        {
            if (textPrefab == null) return;
            Transform canvasTransform = FindFirstObjectByType<Canvas>().transform;
            GameObject t = Instantiate(textPrefab, textSpawnPoint.position, Quaternion.identity, canvasTransform);
            t.GetComponent<FloatingText>().SetText(text, color);
        }

        IEnumerator FlashAndKnockbackRoutine()
        {
            Vector3 startPos = transform.position;
            // Рассчитываем позицию "сзади" (для 2D это обычно минус по X, если враг справа)
            Vector3 targetKnockbackPos = startPos - new Vector3(knockbackDistance, 0, 0);

            // --- 1. ВСПЫШКА (Начало) ---
            // Если у нас Shader Graph, мы просто подменяем материал на секунду
            if (flashMaterial != null)
            {
                // Настраиваем цвет в материале перед применением
                flashMaterial.SetColor("_FlashColor", flashColor);
                flashMaterial.SetFloat("_FlashAmount", 1f); // Полностью белый
                spriteRenderer.material = flashMaterial;
            }

            // --- 2. ОТТАЛКИВАНИЕ НАЗАД ---
            float elapsed = 0f;
            // Duration Knockback = flashDuration
            while (elapsed < flashDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetKnockbackPos, elapsed / flashDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetKnockbackPos; // Гарантируем позицию

            // --- 3. ВСПЫШКА (Конец) ---
            // Возвращаем оригинальный материал
            spriteRenderer.material = originalMaterial;

            // --- 4. ПЛАВНОЕ ВОЗВРАЩЕНИЕ ---
            float returnElapsed = 0f;
            float returnDuration = flashDuration * 2f; // Назад летим быстро, возвращаемся плавнее

            while (returnElapsed < returnDuration)
            {
                // Используем SmoothStep для более "мягкого" возвращения
                float t = returnElapsed / returnDuration;
                float smoothT = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(targetKnockbackPos, startPos, smoothT);
                returnElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = startPos; // Вернулись на место
        }
    }
}