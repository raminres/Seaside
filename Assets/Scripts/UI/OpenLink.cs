using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string url;

    public void OpenLinks()
    {
        Application.OpenURL(url);
    }
    
}
