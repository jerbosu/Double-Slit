using UnityEngine;
using System.Collections;

public class AttackTelegraph : MonoBehaviour
{
    public Vector2 size = new Vector2(1f, 3f);
    public float duration = 1f;
    public Color emptyColor = new Color(1f, 0f, 0f, 0.2f);
    public Color fullColor = new Color(1f, 0f, 0f, 0.6f);

    private SpriteRenderer outline;     // attack indicator outline child sprite
    private SpriteRenderer fill;        // attack indicator filler child sprite

    void Awake()
    {
        // outline
        GameObject outlineObj = new GameObject("Outline");      // make a new gameobject
        outlineObj.transform.SetParent(transform);              // set its parent to the prefab
        outlineObj.transform.localPosition = Vector3.zero;      // pos relative to parent
        outline = outlineObj.AddComponent<SpriteRenderer>();    // add SpriteRenderer component to gameobject
        outline.sprite = GetDefaultSprite();                    // get default unity square
        outline.color = emptyColor;                             // yeah
        outline.transform.localScale = new Vector3(size.x, size.y, 1f);

        // fill starts at zero height
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0f, -size.y * 0.5f, 0f); // anchor to bottom
        fill = fillObj.AddComponent<SpriteRenderer>();
        fill.sprite = GetDefaultSprite();
        fill.color = fullColor;
        fill.transform.localScale = new Vector3(size.x, 0f, 1f);
        fill.sortingOrder = 1; // render on top of outline

        StartCoroutine(Fill());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }










    /* NON UNITY FUNCTIONS */

    IEnumerator Fill()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // grow fill upward from bottom
            float fillHeight = size.y * progress;
            fill.transform.localScale = new Vector3(size.x, fillHeight, 1f);
            fill.transform.localPosition = new Vector3(0f, -size.y * 0.5f + fillHeight * 0.5f, 0f);

            yield return null;
        }
    }

    Sprite GetDefaultSprite()
    {
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    }
}
