using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadarIcon : MonoBehaviour
{
    public Image image;
    private NutrientTesterScript poolOwner;
    private Coroutine fadeRoutine;

    public void Init(NutrientTesterScript owner)
    {
        poolOwner = owner;
        ResetIcon();
    }

    public void ResetIcon()
    {
        gameObject.SetActive(true);
        if (image != null)
        {
            var c = image.color;
            image.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    public void FadeOut(float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCoroutine(duration));
    }

    private IEnumerator FadeCoroutine(float duration)
    {
        float t = 0f;
        Color start = image.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);
            image.color = new Color(start.r, start.g, start.b, alpha);
            yield return null;
        }

        poolOwner.ReturnToPool(this);
    }
}
