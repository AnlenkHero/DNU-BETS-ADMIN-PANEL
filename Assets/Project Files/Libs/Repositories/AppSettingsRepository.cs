using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using Libs.Models;
using Project_Files.Libs;

namespace Libs.Repositories
{
    public static class AppSettingsRepository
    {
        public static IPromise<AppSettings> GetAppSettings()
        {
            return new Promise<AppSettings>((resolve, reject) =>
            {
                string queryUrl = $"{Config.FirebaseDbUrl}appSettings.json";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var appSettings = JsonConvert.DeserializeObject<AppSettings>(response.Text);

                    if (appSettings == null)
                    {
                        reject(new Exception("No app settings found"));
                        return;
                    }

                    resolve(appSettings);
                }).Catch(error => { reject(new Exception($"Error retrieving app settings: {error.Message}")); });
            });
        }

        public static IPromise<string> SaveAppSettings(AppSettings appSettings)
        {
            var promise = new Promise<string>();

            if (appSettings.buffPrice <= 0)
            {
                promise.Reject(new Exception("Buff price is invalid"));
                return promise;
            }

            RestClient.Post($"{Config.FirebaseDbUrl}appSettings.json", appSettings).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("name", out string name))
                {
                    promise.Resolve(name);
                }
                else
                {
                    promise.Reject(new Exception("No response from Firebase"));
                }
            }).Catch(error => { promise.Reject(error); });

            return promise;
        }
        
    }
}
