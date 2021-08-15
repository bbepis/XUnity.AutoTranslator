using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.SceneManagement
{
   public struct Scene
   {
      internal enum LoadingState
      {
         NotLoaded,
         Loading,
         Loaded
      }

      private int m_Handle;

      internal int handle => m_Handle;

      internal LoadingState loadingState => GetLoadingStateInternal( handle );

      public string path => GetPathInternal( handle );

      public string name
      {
         get
         {
            return GetNameInternal( handle );
         }
         internal set
         {
            SetNameInternal( handle, value );
         }
      }

      internal string guid => GetGUIDInternal( handle );

      public bool isLoaded => GetIsLoadedInternal( handle );

      public int buildIndex => GetBuildIndexInternal( handle );

      public bool isDirty => GetIsDirtyInternal( handle );

      public int rootCount => GetRootCountInternal( handle );

      public bool IsValid()
      {
         return IsValidInternal( handle );
      }

      public GameObject[] GetRootGameObjects()
      {
         List<GameObject> list = new List<GameObject>( rootCount );
         GetRootGameObjects( list );
         return list.ToArray();
      }

      public void GetRootGameObjects( List<GameObject> rootGameObjects )
      {
         if( rootGameObjects.Capacity < rootCount )
         {
            rootGameObjects.Capacity = rootCount;
         }

         rootGameObjects.Clear();
         if( !IsValid() )
         {
            throw new ArgumentException( "The scene is invalid." );
         }

         if( !isLoaded )
         {
            throw new ArgumentException( "The scene is not loaded." );
         }

         if( rootCount != 0 )
         {
            GetRootGameObjectsInternal( handle, rootGameObjects );
         }
      }

      public static bool operator ==( Scene lhs, Scene rhs )
      {
         return lhs.handle == rhs.handle;
      }

      public static bool operator !=( Scene lhs, Scene rhs )
      {
         return lhs.handle != rhs.handle;
      }

      public override int GetHashCode()
      {
         return m_Handle;
      }

      public override bool Equals( object other )
      {
         if( !( other is Scene ) )
         {
            return false;
         }

         Scene scene = (Scene)other;
         return handle == scene.handle;
      }

      private static extern bool IsValidInternal( int sceneHandle );

      private static extern string GetPathInternal( int sceneHandle );

      private static extern string GetNameInternal( int sceneHandle );

      private static extern void SetNameInternal( int sceneHandle, string name );

      private static extern string GetGUIDInternal( int sceneHandle );

      private static extern bool GetIsLoadedInternal( int sceneHandle );

      private static extern LoadingState GetLoadingStateInternal( int sceneHandle );

      private static extern bool GetIsDirtyInternal( int sceneHandle );

      private static extern int GetBuildIndexInternal( int sceneHandle );

      private static extern int GetRootCountInternal( int sceneHandle );

      private static extern void GetRootGameObjectsInternal( int sceneHandle, object resultRootList );
   }
}
