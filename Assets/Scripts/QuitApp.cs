using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] 
public class QuitApp : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        Application.Quit();
    }
}