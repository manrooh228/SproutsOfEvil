using System.Collections;
using UnityEngine;

namespace Assets._Scripts.CardSystem
{
    public class CardAnimation : MonoBehaviour
    {
        [Header("Настройки 3D наклона (X, Y)")]
        public float tiltXAmount = 5f;     // Наклон вперед-назад
        public float tiltYAmount = 8f;     // Наклон влево-вправо
        public float tiltZAmount = 2f;     // Наклон по кругу (как раньше)

        [Header("Скорость")]
        public float speed = 1.5f;

        private Quaternion initialRotation;
        private float randomOffset;
        private bool isHovered = false; // Чтобы не мешать, когда мышь наведена

        void Start()
        {
            initialRotation = transform.localRotation;
            randomOffset = Random.Range(0f, 100f);
        }

        void Update()
        {
            // Если мы навели мышь или тащим карту — отключаем Idle-анимацию
            if (isHovered) return;

            float time = (Time.time + randomOffset) * speed;

            // Рассчитываем углы с разными фазами, чтобы движение было "восьмеркой"
            float x = Mathf.Sin(time * 0.8f) * tiltXAmount;
            float y = Mathf.Cos(time * 1.1f) * tiltYAmount;
            float z = Mathf.Sin(time * 0.5f) * tiltZAmount;

            // Применяем вращение относительно начального состояния в руке
            transform.localRotation = initialRotation * Quaternion.Euler(x, y, z);
        }

        // Методы для связи с CardHover / CardDraggable
        public void SetHover(bool state)
        {
            isHovered = state;
            if (!state) ResetRotation(); // Возвращаем в строй, когда убрали мышь
        }

        public void ResetRotation() => transform.localRotation = initialRotation;

        public void SetNewIdleRotation(Quaternion rot) => initialRotation = rot;
    }
}