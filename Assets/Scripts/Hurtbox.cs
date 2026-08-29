using Unity.Mathematics;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health { get; private set; } // readable outside, only writable inside
    public System.Action onDeath;

    private Rigidbody2D parent;

    void Start()
    {
        health = maxHealth;
        parent = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }





    public void TakeDamage(float amount, Vector2 knockback)
    {
        health -= amount;
        parent.AddForce(knockback, ForceMode2D.Impulse);
        Debug.Log(gameObject.name + " health: " + health);
        if (health <= 0)
        {
            onDeath?.Invoke();
        }
    }
}
