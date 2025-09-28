using TMPro;
using UnityEngine;

public class KeyboardButton : MonoBehaviour
{
    private string letter;

    private void Start()
    {
        letter = GetComponent<TextMeshProUGUI>().text;
    }
}
