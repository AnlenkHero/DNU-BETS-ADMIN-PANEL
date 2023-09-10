using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AnotherFileBrowser.Windows;
using UnityEngine.Networking;

public class FileManager : MonoBehaviour
{
   [SerializeField] private RawImage rawImage;
   public static event Action OnImageSelected;
   public void OpenFileBrowser()
   {
      var bp = new BrowserProperties();
      bp.filter = "JPEG/PNG Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

      bp.filterIndex = 0;
      
      new FileBrowser().OpenFileBrowser(bp, path =>
      {
         StartCoroutine(LoadImage(path));
      });
   }

   IEnumerator LoadImage(string path)
   {
      UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
      {
         yield return uwr.SendWebRequest();
         if (uwr.result != UnityWebRequest.Result.Success)
         {
            Debug.Log($"Failed to get image from file browser {uwr.error}");
         }
         else
         {
            var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
            rawImage.texture = uwrTexture;
            OnImageSelected?.Invoke();
         }
      }
   }
}
