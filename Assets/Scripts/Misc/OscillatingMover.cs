using UnityEngine;
using System.Collections;

public class OscillatingMover : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public float duration = 2f;

   
    public bool pauseAtEnds = false;
    public float pauseDuration = 1f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float timer = 0f;
    private bool movingToEnd = true;
    private bool isPaused = false;

    private void Start()
    {
        startPos = transform.position;
        endPos = startPos + offset;
    }

    private void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        if (movingToEnd)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
        }
        else
        {
            transform.position = Vector3.Lerp(endPos, startPos, t);
        }

        if (t >= 1f)
        {
            timer = 0f;
            movingToEnd = !movingToEnd;

            if (pauseAtEnds)
            {
                StartCoroutine(PauseBeforeNextMove());
            }
        }
    }

    private IEnumerator PauseBeforeNextMove()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }
}
