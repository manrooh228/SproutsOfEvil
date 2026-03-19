using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.CardSystem
{
    public class ShaderCode : MonoBehaviour
    {
        Image image;
        Material m;
        // Нам нужна ссылка на данные карты
        private CardDisplay cardDisplay;

        void Start()
        {
            image = GetComponent<Image>();
            // Создаем экземпляр материала, чтобы эффект был только у ЭТОЙ карты
            m = new Material(image.material);
            image.material = m;

            // Ищем скрипт с данными (он обычно на том же объекте или родителе)
            cardDisplay = GetComponentInParent<CardDisplay>();

            ApplyRarityEffect();
        }

        void ApplyRarityEffect()
        {
            // 1. Сначала выключаем все старые эффекты
            foreach (var keyword in image.material.enabledKeywords)
            {
                image.material.DisableKeyword(keyword);
            }

            // 2. Проверяем редкость через данные карты
            if (cardDisplay != null && cardDisplay.cardData != null)
            {
                if (cardDisplay.cardData.rarity == CardRarity.Epic)
                {
                    // Если EPIC — принудительно ставим POLYCHROME (твой монохром/радуга)
                    image.material.EnableKeyword("_EDITION_POLYCHROME");
                    Debug.Log($"Эффект Epic применен к {cardDisplay.cardData.cardName}");
                }
                else
                {
                    // Для остальных (Common/Rare) можно оставить REGULAR или рандом
                    image.material.EnableKeyword("_EDITION_REGULAR");
                }
            }
        }

        void Update()
        {
            // Логика поворота для шейдера (наклоны X и Y)
            if (transform.parent != null)
            {
                Quaternion currentRotation = transform.parent.localRotation;
                Vector3 eulerAngles = currentRotation.eulerAngles;

                float xAngle = ClampAngle(eulerAngles.x, -90f, 90f);
                float yAngle = ClampAngle(eulerAngles.y, -90f, 90f);

                // Передаем углы в шейдер для динамических бликов
                m.SetVector("_Rotation", new Vector2(
                    Remap(xAngle, -20, 20, -.5f, .5f),
                    Remap(yAngle, -20, 20, -.5f, .5f)
                ));
            }
        }

        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        float ClampAngle(float angle, float min, float max)
        {
            if (angle < -180f) angle += 360f;
            if (angle > 180f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        
    }
}