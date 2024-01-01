using System;
using System.Globalization;
using JetBrains.Annotations;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContestantFormView : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField coefficientField;
    [SerializeField] private Toggle isWinnerToggle;
    private ToggleGroup winnerToggleGroup;
    public string Name => nameField.text;
    public double Coefficient => double.TryParse(coefficientField.text,NumberStyles.Any, CultureInfo.InvariantCulture,out double value) ? value : -1;
    public bool IsWinner
    {
        get => isWinnerToggle.isOn;
        set => isWinnerToggle.isOn = value;
    }
    public ToggleGroup WinnerToggleGroup { set => isWinnerToggle.group = value; }

    public void SetData(Contestant contestant = null)
    {
        if (contestant == null) return;
        
        nameField.text = contestant.Name;
        coefficientField.text = contestant.Coefficient.ToString(CultureInfo.InvariantCulture);
        isWinnerToggle.isOn = contestant.Winner;
    }

    public void AddConfirmation([CanBeNull] Action callback = null)
    {
        isWinnerToggle.onValueChanged.AddListener(_ => callback?.Invoke());
    }
}