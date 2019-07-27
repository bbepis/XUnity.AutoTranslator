using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core;

namespace XUnity.AutoTranslator.Plugin.Utilities
{
#pragma warning disable CS0618 // Type or member is obsolete

   internal static class SceneManagerHelper
   {
      public static int GetActiveSceneId()
      {
         if( Features.SupportsSceneManager )
         {
            return GetActiveSceneIdBySceneManager();
         }
         return GetActiveSceneIdByApplication();
      }

      private static int GetActiveSceneIdBySceneManager()
      {
         return SceneManager.GetActiveScene().buildIndex;
      }

      private static int GetActiveSceneIdByApplication()
      {
         return Application.loadedLevel;
      }
   }

   internal class SceneLoadInformation
   {
      public SceneLoadInformation()
      {
         LoadedScenes = new List<SceneInformation>();

         if( Features.SupportsSceneManager )
         {
            LoadBySceneManager();
         }
         else
         {
            LoadByApplication();
         }
      }

      public SceneInformation ActiveScene { get; set; }

      public List<SceneInformation> LoadedScenes { get; set; }

      public void LoadBySceneManager()
      {
         var activeScene = SceneManager.GetActiveScene();

         ActiveScene = new SceneInformation( activeScene.buildIndex, activeScene.name );

         for( int i = 0; i < SceneManager.sceneCount; i++ )
         {
            var scene = SceneManager.GetSceneAt( i );

            LoadedScenes.Add( new SceneInformation( scene.buildIndex, scene.name ) );
         }
      }

      public void LoadByApplication()
      {
         ActiveScene = new SceneInformation( Application.loadedLevel, Application.loadedLevelName );

         LoadedScenes.Add( new SceneInformation( Application.loadedLevel, Application.loadedLevelName ) );
      }
   }

   internal class SceneInformation
   {
      public SceneInformation( int id, string name )
      {
         Id = id;
         Name = name;
      }

      public int Id { get; set; }

      public string Name { get; set; }
   }
}
