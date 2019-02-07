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
         if( Features.SupportsSceneManager )
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
#pragma warning disable CS0618 // Type or member is obsolete
         return Application.loadedLevel.ToString();
#pragma warning restore CS0618 // Type or member is obsolete
      }
   }
}
