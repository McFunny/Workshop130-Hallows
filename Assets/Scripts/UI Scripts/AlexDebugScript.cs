using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlexDebugScript : MonoBehaviour
{
    public static AlexDebugScript Instance;
    public TextMeshProUGUI horizontalText;
    public TextMeshProUGUI verticalText;
    public TextMeshProUGUI xDirText, yDirText;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetMovementVector(Vector2 vector1, Vector2 vector2)     
    {
        horizontalText.text = "Horizontal: " + vector1.x.ToString("F2");
        verticalText.text = "Vertical: " + vector1.y.ToString("F2");
        xDirText.text = "X Dir: " + vector2.x.ToString("F2");
        yDirText.text = "Y Dir: " + vector2.y.ToString("F2");
    }
}
