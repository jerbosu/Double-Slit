using System.Collections;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    // stats
    public float maxHealth = 100f;  // SET IN INSPECTOR
    public float health { get; private set; } // readable outside, only writable inside

    // hit effects
    public Color flashColor = Color.white;  // also set in inspector
    public float flashDuration = 0.15f;
    public SpriteRenderer flashOverlay;    // new sr on top of original sprite so original doesnt need to be changed
    public ParticleSystem hitParticles;

    // misc
    public System.Action onDeath;   // event that other scripts can listen to

    private Rigidbody2D parent;     // for applying knockback



    /* Unity functions */

    void Start()
    {
        health = maxHealth;
        parent = GetComponentInParent<Rigidbody2D>();
    }








    /* Other functions */

    public void TakeDamage(float amount, Vector2 knockback)
    {
        health -= amount;
        parent.AddForce(knockback, ForceMode2D.Impulse);

        if (flashOverlay != null) { StartCoroutine(Flash()); }

        if (hitParticles != null)
        {
            var main = hitParticles.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(amount * 2.5f, amount * 3.5f);    // scale particle speed with damage amount

            float angle = Mathf.Atan2(knockback.y, knockback.x) * Mathf.Rad2Deg - hitParticles.shape.arc / 2;
            hitParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var shape = hitParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f; // spread angle

            hitParticles.Emit(5);
        }

        Debug.Log(gameObject.name + " health: " + health);
        if (health <= 0)
        {
            onDeath?.Invoke();
            health = 0;
        }
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        // Debug.Log("Healed " + amount);
    }


    IEnumerator Flash()
    {
        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            // float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            // flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }
        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
}
