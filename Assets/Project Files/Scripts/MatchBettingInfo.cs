using Libs.Helpers;
using TMPro;
using UnityEngine;

public class MatchBettingInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contestantName;
    [SerializeField] private TextMeshProUGUI totalBetAmount;

    public void SetData(string contestant, double totalBet)
    {
        contestantName.text = contestant;
        totalBetAmount.text = $"{totalBet.ToString()}<color={ColorHelper.LightGreenString}>$</color>";
    }
}