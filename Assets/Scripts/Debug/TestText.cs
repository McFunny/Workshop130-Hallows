using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestText : MonoBehaviour
{
    public TextMeshProUGUI tokenText;

    // Update is called once per frame
    void Update()
    {
        tokenText.text = PlayerMovement.restrictMovementTokens.ToString();
    }
}
