using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.BattleSystem
{
    public class Enemy : MonoBehaviour
    {
        public int health = 50;
        public TextMeshProUGUI healthText;

        public void TakeDamage(int damage)
        {
            health -= damage;
            healthText.text = health.ToString();
            if (health <= 0) Die();
        }

        void Die() => Debug.Log("Враг повержен!");
    }
}