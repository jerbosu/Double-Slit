using UnityEngine;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;     // itself

    private UnityEngine.UI.Image image;     // idk

    void Awake()
    {
        Instance = this;
        image = GetComponent<UnityEngine.UI.Image>();
    }

    // to be called by other classes
    public void Flash(Color color, float duration, bool fade)
    {
        StartCoroutine(DoFlash(color, duration, fade));
    }

    /// <summary>
    /// Flash the screen color color for duration seconds.
    /// </summary>
    /// <param name="color">RGBA 0-1</param>
    /// <param name="duration">Duration in seconds</param>
    /// <returns></returns>
    IEnumerator DoFlash(Color color, float duration, bool fade)
    {
        float setAlpha = color.a;
        image.color = color;

        yield return new WaitForSecondsRealtime(duration);

        // lerp flash to transparent
        if (fade == true)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(setAlpha, 0f, elapsed / duration);
                image.color = color;
                yield return null;
            }
        }

        color.a = 0f;
        image.color = color;
    }
}
