using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnim : MonoBehaviour
{
    public Image image;
    public Sprite[] spriteArray;
    public bool hideOnStart = true;
    public bool hideOnComplete = true;
    public float timeBetweenFrames = .02f;
    private int indexSprite;
    Coroutine corotineAnim;    
    bool IsDone;

    private void Start()
    {
        if(hideOnStart) image.enabled = false;
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

    public void PlayUI()
    { 
        IsDone = false;
        corotineAnim = StartCoroutine(PlayAnimCoroutineUI(false));
    }
    
    public void PlayOneShotUI()
    {
        IsDone = false;
        corotineAnim = StartCoroutine(PlayAnimCoroutineUI(true));
    }

    public void StopUI()
    {      
        IsDone = true;
        StopCoroutine(corotineAnim);
        ResetSprite();
    }

    private void ResetSprite()
    {
        if(hideOnComplete) image.enabled = false;
        indexSprite = 0;
        image.sprite = spriteArray[indexSprite];
    }

    IEnumerator PlayAnimCoroutineUI(bool playOnce)
    {
        image.enabled = true;
        while (!IsDone)
        {
            yield return new WaitForSeconds(timeBetweenFrames);

            if (indexSprite >= spriteArray.Length)
            {
                indexSprite = 0;
                if (playOnce)
                {
                    IsDone = true;
                    ResetSprite();
                    break;
                }
            }

            image.sprite = spriteArray[indexSprite];
            indexSprite += 1;
        }
    }
}