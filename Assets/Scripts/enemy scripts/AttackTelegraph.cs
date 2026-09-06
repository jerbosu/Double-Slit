using UnityEngine;
using System.Collections;

public class AttackTelegraph : MonoBehaviour
{
    public Vector2 size = new Vector2(0.35f, 2.5f);          // telegraph size
    public float duration = 1f;
    public float lockTime = 0.3f;
    public Color color = new Color(1f, 0f, 0f, 0.1f);   // transparent red

    private SpriteRenderer sr;
    private Transform enemy;
    private Transform player;
    private bool locked = false;
    private Vector2? targetOverride = null;

    private static Sprite _defaultSprite;

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = color;
    }

    public void Init(Transform enemyTransform, Transform playerTransform)
    {
        enemy = enemyTransform;
        sr.sortingOrder = enemyTransform.Find("sprite").GetComponentInChildren<SpriteRenderer>().sortingOrder - 1;
        player = playerTransform;
        StartCoroutine(Run());
        transform.localScale = new Vector3(size.x, size.y, 1f);     // resize in Init cuz Awake is too early
    }

    IEnumerator Run()
    {
        float elapsed = 0f;
        float trackDuration = duration - lockTime;

        // track player for most of the windup
        while (elapsed < trackDuration)
        {
            AimAtPlayer();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // lock in place for final lockTime seconds
        locked = true;
        yield return new WaitForSeconds(lockTime);
    }

    void AimAtPlayer()
    {
        if (enemy == null || player == null) return;

        Vector2 target = targetOverride ?? (Vector2)player.position;

        transform.position = enemy.position;
        Vector2 direction = (target - (Vector2)enemy.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    public void SetTargetOverride(Vector2 target)
    {
        targetOverride = target;
    }

    void Update()
    {
        if (!locked)
            AimAtPlayer();
    }

    Sprite GetDefaultSprite()
    {
        if (_defaultSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _defaultSprite = Sprite.Create(
                tex,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0f),
                1f // pixels per unit = 1, so 1 pixel = 1 Unity unit
            );
        }
        return _defaultSprite;
    }
}