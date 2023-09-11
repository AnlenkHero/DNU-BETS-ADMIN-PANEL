using System;
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
    [SerializeField] private Button button;
    public void ShowPanel(Color color,string header,[CanBeNull] string textForButton=null,[CanBeNull] string info=null,[CanBeNull] Action buttonAction=null)
    {
        headerText.text = header;
        infoText.color = color;
        headerText.color = color;
        infoText.text = info ?? "";
        buttonText.text = textForButton ?? "hide";
        button.onClick.AddListener(() =>
        {
            buttonAction?.Invoke();
        });
        panel.SetActive(true);
    }
} 
