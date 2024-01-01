using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;


public class InfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Transform buttonsGrid;
    [SerializeField] private EmptyButton emptyButton;
    public void ShowPanel(Color color,string header,[CanBeNull] string info=null,[CanBeNull] Action callback=null)
    {
        StartCoroutine(ClearExistingButtons());
        headerText.text = header;
        infoText.color = color;
        headerText.color = color;
        infoText.text = info ?? "";
        panel.SetActive(true);
        if (callback == null)
        {
            AddButton("Close", HidePanel);
        }
        callback?.Invoke();
    }

    public void AddButton(string buttonText, [CanBeNull] Action buttonAction = null, [CanBeNull] string buttonColorString = null)
    {
        var button = Instantiate(emptyButton, buttonsGrid);
        button.SetData(buttonText,buttonAction, buttonColorString);
    }
    
    private IEnumerator ClearExistingButtons()
    {
        foreach (Transform child in buttonsGrid)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForEndOfFrame();
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }
} 
