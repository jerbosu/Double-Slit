using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // drag + drop these into an attack prefab and customize in inspector
    public float damage = 10f;
    public float knockbackForce = 10f;

    private CapsuleCollider2D col;
    private Rigidbody2D parent;

    void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();    // get the collider
    }

    void Start()
    {
        parent = GetComponentInParent<Rigidbody2D>();
    }

    // for if an attack hitbox changes over the attack duration (i.e. heavy attack)
    public void SetShape(Vector2 size, Vector2 offset)
    {
        col.size = size;
        col.offset = offset;
    }

    // Unity auto calls this when colliders collide (wow)
    void OnTriggerEnter2D(Collider2D other)
    {
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();    // get the hurtbox of the colliding gameobject
        if (hurtbox != null)
        {
            Vector2 knockback = (other.transform.position - transform.position).normalized * knockbackForce;
            parent.AddForce(-knockback * 0.5f, ForceMode2D.Impulse);
            hurtbox.TakeDamage(damage, knockback);
        }
    }
}