using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughtBubble : MonoBehaviour
{
    [System.Serializable]
    public class EmotionSprites
    {
        public string name;
        public Sprite sprite1, sprite2;
    }

    public EmotionSprites[] emotionSprites;

    public Animator anim;
    public SpriteRenderer bubble, emotion;

    bool isPlaying;
    int currentIndex;

    void Start()
    {
        bubble.enabled = false;
        emotion.enabled = false;
    }

    public void PlayEmotion(int index)
    {
        if(isPlaying) return;
        isPlaying = true;
        currentIndex = index;
        StartCoroutine(PlayEmotion());
    }

    public IEnumerator PlayEmotion()
    {
        bubble.enabled = true;
        anim.SetBool("IsPlaying", true);
        yield return new WaitForSeconds(0.4f);
        emotion.enabled = true;
        StartCoroutine(AnimateEmotion());
        yield return new WaitForSeconds(3);
        emotion.enabled = false;
        anim.SetBool("IsPlaying", false);
        yield return new WaitForSeconds(0.4f);
        bubble.enabled = false;
        yield return new WaitForSeconds(1f);
        isPlaying = false;
    }

    IEnumerator AnimateEmotion()
    {
        while(emotion.enabled)
        {
            emotion.sprite = emotionSprites[currentIndex].sprite1;
            yield return new WaitForSeconds(0.5f);
            emotion.sprite = emotionSprites[currentIndex].sprite2;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
