using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public Slider healthBar;
    public Hurtbox playerHurtbox;

    void Update()
    {
        healthBar.value = playerHurtbox.health;
        healthBar.maxValue = playerHurtbox.maxHealth;
    }
}