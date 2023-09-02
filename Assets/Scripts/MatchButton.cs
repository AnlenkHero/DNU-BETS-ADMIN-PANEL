using UnityEngine;
using UnityEngine.UI;

public class MatchButton : MonoBehaviour
{
    [SerializeField] private GameObject editPanel;
    [SerializeField] private Camera camera;
    [SerializeField] private Button button;
    private void Awake()
    {
        button.onClick.AddListener(MoveToEdit);
    }

    private void MoveToEdit()
    {
        camera.transform.position = editPanel.transform.position;
    }
}
