using TMPro;
using UnityEngine;

public class LoadSaveButton : MonoBehaviour
{
    private UIMenuButton uIMenuButton;
    public TextMeshProUGUI[] textBoxes; //0 No save, 1 Save Stats
    Color c_deselected, c_selected;
    // Start is called before the first frame update
    void Start()
    {
        uIMenuButton = GetComponent<UIMenuButton>();
        c_selected = new Color(1f, 0.8870801f, 0.2877358f, 1.0f);
        c_deselected = new Color(0.8509804f, 0.7490196f, 0.2078431f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(uIMenuButton.GetSelected())
        {
            /*for(int i = 0; i < textBoxes.Length; i++)
            {
                textBoxes[i].color = c_selected;
            }*/
            textBoxes[1].color = c_selected;
        }
        else
        {
            /*for(int i = 0; i < textBoxes.Length; i++)
            {
                textBoxes[i].color = c_deselected;
            }*/
            textBoxes[1].color = c_deselected;
        }
    }
}
