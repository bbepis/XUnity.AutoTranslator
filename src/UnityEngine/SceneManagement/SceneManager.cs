using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.SceneManagement
{
   public class SceneManager
   {
      public static int sceneCount
      {
         get;
      }

      public static int sceneCountInBuildSettings
      {
         get;
      }

#if IL2CPP
      public static unsafe UnityAction<Scene, LoadSceneMode> sceneLoaded
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }
#else
      public static event UnityAction<Scene, LoadSceneMode> sceneLoaded;
#endif


      public static Scene GetActiveScene()
      {
         INTERNAL_CALL_GetActiveScene( out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetActiveScene( out Scene value );

      public static bool SetActiveScene( Scene scene )
      {
         return INTERNAL_CALL_SetActiveScene( ref scene );
      }

      private static extern bool INTERNAL_CALL_SetActiveScene( ref Scene scene );

      public static Scene GetSceneByPath( string scenePath )
      {
         INTERNAL_CALL_GetSceneByPath( scenePath, out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetSceneByPath( string scenePath, out Scene value );

      public static Scene GetSceneByName( string name )
      {
         INTERNAL_CALL_GetSceneByName( name, out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetSceneByName( string name, out Scene value );

      public static Scene GetSceneByBuildIndex( int buildIndex )
      {
         INTERNAL_CALL_GetSceneByBuildIndex( buildIndex, out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetSceneByBuildIndex( int buildIndex, out Scene value );

      public static Scene GetSceneAt( int index )
      {
         INTERNAL_CALL_GetSceneAt( index, out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetSceneAt( int index, out Scene value );

      [Obsolete( "Use SceneManager.sceneCount and SceneManager.GetSceneAt(int index) to loop the all scenes instead." )]
      public static Scene[] GetAllScenes()
      {
         Scene[] array = new Scene[ sceneCount ];
         for( int i = 0 ; i < sceneCount ; i++ )
         {
            ref Scene reference = ref array[ i ];
            reference = GetSceneAt( i );
         }

         return array;
      }

      public static void LoadScene( string sceneName )
      {
         LoadSceneMode mode = LoadSceneMode.Single;
         LoadScene( sceneName, mode );
      }

      public static void LoadScene( string sceneName, LoadSceneMode mode )
      {
         LoadSceneAsyncNameIndexInternal( sceneName, -1, ( mode == LoadSceneMode.Additive ) ? true : false, mustCompleteNextFrame: true );
      }

      public static void LoadScene( int sceneBuildIndex )
      {
         LoadSceneMode mode = LoadSceneMode.Single;
         LoadScene( sceneBuildIndex, mode );
      }

      public static void LoadScene( int sceneBuildIndex, LoadSceneMode mode )
      {
         LoadSceneAsyncNameIndexInternal( null, sceneBuildIndex, ( mode == LoadSceneMode.Additive ) ? true : false, mustCompleteNextFrame: true );
      }

      public static AsyncOperation LoadSceneAsync( string sceneName )
      {
         LoadSceneMode mode = LoadSceneMode.Single;
         return LoadSceneAsync( sceneName, mode );
      }

      public static AsyncOperation LoadSceneAsync( string sceneName, LoadSceneMode mode )
      {
         return LoadSceneAsyncNameIndexInternal( sceneName, -1, ( mode == LoadSceneMode.Additive ) ? true : false, mustCompleteNextFrame: false );
      }

      public static AsyncOperation LoadSceneAsync( int sceneBuildIndex )
      {
         LoadSceneMode mode = LoadSceneMode.Single;
         return LoadSceneAsync( sceneBuildIndex, mode );
      }

      public static AsyncOperation LoadSceneAsync( int sceneBuildIndex, LoadSceneMode mode )
      {
         return LoadSceneAsyncNameIndexInternal( null, sceneBuildIndex, ( mode == LoadSceneMode.Additive ) ? true : false, mustCompleteNextFrame: false );
      }

      private static extern AsyncOperation LoadSceneAsyncNameIndexInternal( string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame );

      public static Scene CreateScene( string sceneName )
      {
         INTERNAL_CALL_CreateScene( sceneName, out Scene value );
         return value;
      }

      private static extern void INTERNAL_CALL_CreateScene( string sceneName, out Scene value );

      [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
      public static bool UnloadScene( Scene scene )
      {
         return UnloadSceneInternal( scene );
      }

      private static bool UnloadSceneInternal( Scene scene )
      {
         return INTERNAL_CALL_UnloadSceneInternal( ref scene );
      }

      private static extern bool INTERNAL_CALL_UnloadSceneInternal( ref Scene scene );

      [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
      public static bool UnloadScene( int sceneBuildIndex )
      {
         UnloadSceneNameIndexInternal( "", sceneBuildIndex, immediately: true, out bool outSuccess );
         return outSuccess;
      }

      [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
      public static bool UnloadScene( string sceneName )
      {
         UnloadSceneNameIndexInternal( sceneName, -1, immediately: true, out bool outSuccess );
         return outSuccess;
      }

      public static AsyncOperation UnloadSceneAsync( int sceneBuildIndex )
      {
         bool outSuccess;
         return UnloadSceneNameIndexInternal( "", sceneBuildIndex, immediately: false, out outSuccess );
      }

      public static AsyncOperation UnloadSceneAsync( string sceneName )
      {
         bool outSuccess;
         return UnloadSceneNameIndexInternal( sceneName, -1, immediately: false, out outSuccess );
      }

      public static AsyncOperation UnloadSceneAsync( Scene scene )
      {
         return UnloadSceneAsyncInternal( scene );
      }

      private static AsyncOperation UnloadSceneAsyncInternal( Scene scene )
      {
         return INTERNAL_CALL_UnloadSceneAsyncInternal( ref scene );
      }

      private static extern AsyncOperation INTERNAL_CALL_UnloadSceneAsyncInternal( ref Scene scene );

      private static extern AsyncOperation UnloadSceneNameIndexInternal( string sceneName, int sceneBuildIndex, bool immediately, out bool outSuccess );

      public static void MergeScenes( Scene sourceScene, Scene destinationScene )
      {
         INTERNAL_CALL_MergeScenes( ref sourceScene, ref destinationScene );
      }

      private static extern void INTERNAL_CALL_MergeScenes( ref Scene sourceScene, ref Scene destinationScene );

      public static void MoveGameObjectToScene( GameObject go, Scene scene )
      {
         INTERNAL_CALL_MoveGameObjectToScene( go, ref scene );
      }

      private static extern void INTERNAL_CALL_MoveGameObjectToScene( GameObject go, ref Scene scene );
   }
}
