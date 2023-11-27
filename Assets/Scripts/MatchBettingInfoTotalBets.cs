using TMPro;
using UnityEngine;

public class MatchBettingInfoTotalBets : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalBetsAmount;

    public void SetData(int totalBet)
    {
        totalBetsAmount.text = totalBet.ToString();
    }
}