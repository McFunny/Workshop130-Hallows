using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class CodexPage : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Image image;

    public abstract void UpdatePage(CodexEntries entry);
    
}
