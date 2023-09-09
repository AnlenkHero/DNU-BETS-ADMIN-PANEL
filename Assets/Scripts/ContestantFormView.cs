using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContestantFormView : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField coefficientField;
    [SerializeField] private Toggle isWinnerToggle;
    public string Name => nameField.text;
    public double Coefficient => double.Parse(coefficientField.text);
    public bool IsWinner => isWinnerToggle.isOn;

    public void SetData(Contestant contestant = null)
    {
        if (contestant == null) return;
        
        nameField.text = contestant.Name;
        coefficientField.text = contestant.Coefficient.ToString();
        isWinnerToggle.isOn = contestant.Winner;
    }
}