using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public sealed class Resources
   {
      internal static T[] ConvertObjects<T>( Object[] rawObjects ) where T : Object
      {
         if( rawObjects == null )
         {
            return null;
         }

         T[] array = new T[ rawObjects.Length ];
         for( int i = 0 ; i < array.Length ; i++ )
         {
            array[ i ] = (T)rawObjects[ i ];
         }

         return array;
      }

      public static extern Object[] FindObjectsOfTypeAll( Type type );

      public static T[] FindObjectsOfTypeAll<T>() where T : Object
      {
         return ConvertObjects<T>( FindObjectsOfTypeAll( typeof( T ) ) );
      }

      public static Object Load( string path )
      {
         return Load( path, typeof( Object ) );
      }

      public static T Load<T>( string path ) where T : Object
      {
         return (T)Load( path, typeof( T ) );
      }

      public static extern Object Load( string path, Type systemTypeInstance );

      public static ResourceRequest LoadAsync( string path )
      {
         return LoadAsync( path, typeof( Object ) );
      }

      public static ResourceRequest LoadAsync<T>( string path ) where T : Object
      {
         return LoadAsync( path, typeof( T ) );
      }

      public static extern ResourceRequest LoadAsync( string path, Type type );

      public static extern Object[] LoadAll( string path, Type systemTypeInstance );

      public static Object[] LoadAll( string path )
      {
         return LoadAll( path, typeof( Object ) );
      }

      public static T[] LoadAll<T>( string path ) where T : Object
      {
         return ConvertObjects<T>( LoadAll( path, typeof( T ) ) );
      }

      public static extern Object GetBuiltinResource( Type type, string path );

      public static T GetBuiltinResource<T>( string path ) where T : Object
      {
         return (T)GetBuiltinResource( typeof( T ), path );
      }

      public static extern void UnloadAsset( Object assetToUnload );

      public static extern AsyncOperation UnloadUnusedAssets();
   }
}
