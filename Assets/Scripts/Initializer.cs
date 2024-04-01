using System.Collections;
using System.Collections.Generic;
using System.Text;
using Libs.Config;
using Proyecto26;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    private static Initializer Instance { get; set; } 
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApiSettings apiSettings = ConfigManager.Settings.ApiSettings;
            
            if (apiSettings.UseAuthentication())
            {
                AuthenticateUser(apiSettings);
                StartCoroutine(StartReAuthenticationTimer(apiSettings));
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void AuthenticateUser(ApiSettings apiSettings)
    {
        var requestHeaders = new Dictionary<string, string>
        {
            { "login", apiSettings.Login },
            { "password", apiSettings.Password }
        };
        
        var requestHelper = new RequestHelper
        {
            Uri = $"{apiSettings.Url}/{apiSettings.LoginEnpoint}",
            Headers = requestHeaders,
            ParseResponseBody = true
        };
        
        RestClient.Post(requestHelper).Then(response =>
        {
            RestClient.DefaultRequestHeaders["Authorization"] = $"Bearer {response.Text}";
        }).Catch(e =>
        {
            Debug.LogError(e.Message);
        });
    }
    
    private IEnumerator StartReAuthenticationTimer(ApiSettings apiSettings)
    {
        while (true)
        {
            yield return new WaitForSeconds(apiSettings.TokenLifeTimeInSeconds);
            AuthenticateUser(apiSettings);
        }
    }
}