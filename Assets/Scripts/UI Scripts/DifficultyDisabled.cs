using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyDisabled : MonoBehaviour
{
    [SerializeField] private Button difficultyButton;
    private Image mainImage;
    // Start is called before the first frame update
    void Start()
    {
        mainImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (difficultyButton.interactable == false)
        {
            mainImage.color = difficultyButton.colors.disabledColor;
        }
        else
        {
            mainImage.color = difficultyButton.colors.normalColor;
        }
    }
}
