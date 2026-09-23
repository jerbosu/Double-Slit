using UnityEngine;

public class Projectile : MonoBehaviour
{

    private Hitbox hitbox;
    private GameObject owner; // the GO that fired the projectile





    void Start()
    {
        hitbox = GetComponent<Hitbox>();
    }

    public void Init(GameObject ownerObj)
    {
        owner = ownerObj;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root.gameObject == owner) return;

        if (other.CompareTag("Wall"))
        {
            // hit wall — just destroy
            Destroy(gameObject);
            return;
        }

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                // hit player
                Destroy(gameObject);
                Debug.Log("Hit player");
                return;
            }

            if (other.transform.root.CompareTag("Enemy"))
            {
                // hit enemy — maybe parry or friendly fire handling
                Destroy(gameObject);
                Debug.Log("Hit enemy");
                return;
            }
        }
    }
}
