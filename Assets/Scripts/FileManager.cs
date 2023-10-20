using System;
using UnityEngine;
using UnityEngine.UI;
using AnotherFileBrowser.Windows;
using Libs.Helpers;

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
         TextureLoader.LoadTexture(this,path,texture2D =>
         {
            if (texture2D != null)
            {
               rawImage.texture = texture2D; 
               OnImageSelected?.Invoke();
            }
            else
            {
               Debug.LogError("Texture failed to load.");
            }
         });
      });
   }
}
