using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    // Unity Actions for number key and scroll input
    public UnityAction<int> OnNumberPressed;
    public UnityAction<int> OnScrollInput;  // New UnityAction for scroll input

    // FOR TOGGLING THE GRID
    public Tilemap structGrid;
    public Color activeColor, hiddenColor;

    void Update()
    {
        CheckForScrollInput();
        CheckNumberInput();

        if (Input.GetKeyDown("g") && structGrid)
        {
            if (structGrid.color == activeColor) structGrid.color = hiddenColor;
            else structGrid.color = activeColor;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void CheckForScrollInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f)
        {
            // Scroll up
            OnScrollInput?.Invoke(-1); 
        }

        if (scrollInput < 0f)
        {
            // Scroll down
            OnScrollInput?.Invoke(1); 
        }
    }

    void CheckNumberInput()
    {
        for (int i = 1; i <= 9; i++)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + i);
            if (Input.GetKeyDown(key))
            {
                // Invoke the action and pass the number that was pressed
                OnNumberPressed?.Invoke(i);
            }
        }
    }
}
