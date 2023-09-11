using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void ShowPanel(Color color,string header,[CanBeNull] string textForButton=null,[CanBeNull] string info=null)
    {
        headerText.text = header;
        infoText.text = info ?? "";
        buttonText.text = textForButton ?? "hide";
        infoText.color = color;
        headerText.color = color;
        panel.SetActive(true);
    }
} 
