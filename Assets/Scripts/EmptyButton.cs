using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmptyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button button;

    public void SetData(string textForButton, [CanBeNull] Action buttonAction = null)
    {
        buttonText.text = textForButton;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            buttonAction?.Invoke();
        });
    }

}