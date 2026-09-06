using UnityEngine;
using System.Collections;

public class Parryable : MonoBehaviour
{
    public bool isParryable = false;    // set to true during the parry window
    public System.Action onParried;     // parry event

    public void Parry()
    {
        if (isParryable)
            onParried?.Invoke();
            ScreenFlash.Instance.Flash(new Color(1f, 1f, 1f, 0.1f), 0.5f);
            StartCoroutine(ParryHitstop());
    }

    IEnumerator ParryHitstop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

}
