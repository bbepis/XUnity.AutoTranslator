using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;

namespace XUnity.AutoTranslator.Plugin.Utilities
{
   internal static class SceneManagerHelper
   {
      public static string GetActiveSceneId()
      {
         if( Features.SupportsScenes )
         {
            return GetActiveSceneIdBySceneManager();
         }
         return GetActiveSceneIdByApplication();
      }

      private static string GetActiveSceneIdBySceneManager()
      {
         return SceneManager.GetActiveScene().ToString();
      }

      private static string GetActiveSceneIdByApplication()
      {
         return Application.loadedLevel.ToString();
      }
   }
}
