using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWebsite : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenFeedbackForm(string URL)
    {
        Application.OpenURL(URL);
    }
}
