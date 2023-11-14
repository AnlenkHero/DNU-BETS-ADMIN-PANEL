using System.Collections.Generic;
using System.Linq;
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
        UserRepository.GetAllUsers().Then(userList =>
        {
            IEnumerable<User> usersWithUnprocessedBuffs = userList.Where(x => x.buffPurchase.Any(x => !x.isProcessed));
            foreach (User user in usersWithUnprocessedBuffs)
            {
                var buffPanel = Instantiate(buffInfoPrefab, buffInfoParent);
                buffPanel.SetData(user, infoPanel);
            }

            if (!usersWithUnprocessedBuffs.Any())
            {
                infoPanel.ShowPanel(Color.red, "No users found",
                    "There are no users with unprocessed buffs",
                    () => { infoPanel.AddButton("Back", () => SceneManager.LoadScene("MatchChooseScene")); });
            }
        });
    }
}