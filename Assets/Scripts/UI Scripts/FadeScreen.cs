using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    public Image image;
    Color imageColor;
    public static bool coverScreen = false;

    public bool delay = true;

    void Start()
    {
        coverScreen = false;
        if(image) imageColor = image.color;

        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    void Update()
    {
        if(!image || delay) return;
        if(coverScreen && imageColor.a < 1)
        {
            imageColor.a += 0.01f;
            image.color = imageColor;
        }

        if(!coverScreen && imageColor.a > 0)
        {
            imageColor.a -= 0.005f;
            image.color = imageColor;
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        delay = false;
    }
}
