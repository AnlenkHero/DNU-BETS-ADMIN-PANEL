using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBuffManager : MonoBehaviour
{
    [SerializeField] private BuffInfoPanel buffInfoPrefab;
    [SerializeField] private Transform buffInfoParent;
    [SerializeField] private InfoPanel infoPanel;

    private void Awake()
    {
        UserRepository.GetAllUsersWithPurchases(isBuffProcessed: false).Then(users =>
        {
            if (users?.Any() != true)
            {
                infoPanel.ShowPanel(ColorHelper.HotPink, "No users found",
                    "There are no users with unprocessed buffs",
                    () => { infoPanel.AddButton("Back", () => SceneManager.LoadScene("MatchChooseScene")); });
                
                return;
            }
            
            foreach (User user in users)
            {
                var buffPanel = Instantiate(buffInfoPrefab, buffInfoParent);
                buffPanel.SetData(user, infoPanel);
            }
        }).Catch(e =>
        {
            Debug.LogError(e.Message);
        });
    }
}