using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    public Image image;
    Color imageColor;
    public static bool coverScreen = false;

    void Start()
    {
        coverScreen = false;
        if(image) imageColor = image.color;
    }

    // Update is called once per frame
    void Update()
    {
        if(!image) return;
        if(coverScreen && imageColor.a < 1)
        {
            imageColor.a += 0.02f;
            image.color = imageColor;
        }

        if(!coverScreen && imageColor.a > 0)
        {
            imageColor.a -= 0.02f;
            image.color = imageColor;
        }
    }
}
