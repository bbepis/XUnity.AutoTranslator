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

   internal class SceneLoadInformation
   {
      public SceneLoadInformation()
      {
         LoadedScenes = new List<SceneInformation>();

         LoadBySceneManager();
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
