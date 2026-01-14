using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSplat : MonoBehaviour
{
    [SerializeField] float lifetime = 1.5f;
    [SerializeField] float fadeTime = 0.5f;

    [Header("Punch")]
    [SerializeField] bool punch = true;
    [SerializeField] float punchDuration = 0.2f;
    [SerializeField] float punchStartScale = 0.6f;
    [SerializeField] float punchOvershoot = 1.15f;
    Vector3 baseScale;
    float punchTimer;

    public Image image;
    public RectTransform rect;
    float timer;

    ScreenSplatPool pool;

    void OnEnable()
    {
        timer = lifetime;
        SetAlpha(1f);

        baseScale = rect.localScale;
        punchTimer = punchDuration;

        if (punch) 
        {
            punchOvershoot = 1.15f;
            rect.localScale = baseScale * punchStartScale;
            punchOvershoot *= Random.Range(0.95f, 1.1f);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= fadeTime)
        {
            float t = Mathf.Clamp01(timer / fadeTime);
            SetAlpha(t);
        }

        if (punch && punchTimer > 0f)     UpdatePunch();

        if (timer <= 0f)
            pool.Release(this);
    }

    void UpdatePunch()
    {
        punchTimer -= Time.deltaTime;

        float t = 1f - (punchTimer / punchDuration);

        // Two-phase curve: grow then settle
        float scaleFactor;
        if (t < 0.5f)
        {
            // Grow to overshoot
            scaleFactor = Mathf.Lerp(punchStartScale, punchOvershoot, t / 0.5f);
        }
        else
        {
            // Settle back to 1
            scaleFactor = Mathf.Lerp(punchOvershoot, 1f, (t - 0.5f) / 0.5f);
        }

        rect.localScale = baseScale * scaleFactor;
    }

    void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }

    public void Init(ScreenSplatPool owningPool)
    {
        pool = owningPool;
    }
}
