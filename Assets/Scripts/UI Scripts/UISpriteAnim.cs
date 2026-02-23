using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnim : MonoBehaviour
{
    public Image image;
    public Sprite[] spriteArray;
    public bool playOnStart = false;
    public bool hideOnStart = true;
    public bool hideOnComplete = true;
    public float timeBetweenFrames = .02f;
    private int indexSprite;
    Coroutine corotineAnim;    
    bool IsDone;

    private void Start()
    {
        if (playOnStart) PlayUI();
        if (hideOnStart) image.enabled = false;
        else image.enabled = true;
    }

    // DEBUG
    /*public void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayAnimUI();
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayOneShotUI();
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            StopAnimUI();
        }
    }*/

    public void PlayUI(Color averageColor = default(Color))
    { 
        if (averageColor == default(Color)) averageColor = image.color;

        image.color = averageColor;
        IsDone = false;
        ResetSprite();
        corotineAnim = StartCoroutine(PlayAnimCoroutineUI(false));
    }
    
    public void PlayOneShotUI(Color averageColor = default(Color))
    {
        if (averageColor == default(Color)) averageColor = image.color;
            
        image.color = averageColor;
        IsDone = false;
        ResetSprite();
        corotineAnim = StartCoroutine(PlayAnimCoroutineUI(true));
    }

    public void StopUI()
    {      
        IsDone = true;
        if(corotineAnim != null) StopCoroutine(corotineAnim);
        if(hideOnComplete) image.enabled = false;
        ResetSprite();
    }

    private void ResetSprite()
    {
        indexSprite = 0;
        image.sprite = spriteArray[indexSprite];
    }

    IEnumerator PlayAnimCoroutineUI(bool playOnce)
    {
        image.enabled = true;
        while (!IsDone)
        {
            yield return new WaitForSeconds(timeBetweenFrames);

            image.sprite = spriteArray[indexSprite];
            indexSprite += 1;

            if (indexSprite >= spriteArray.Length)
            {
                indexSprite = 0;
                if (playOnce)
                {
                    IsDone = true;
                    break;
                }
            }
        }
        if(hideOnComplete) image.enabled = false;
    }
}