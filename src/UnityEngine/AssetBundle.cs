using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if IL2CPP
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace UnityEngine
{
   public sealed class AssetBundle : Object
   {
      public AssetBundle( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Object mainAsset
      {
         get;
      }

      public bool isStreamedSceneAssetBundle
      {
         get;
      }

      public static extern AssetBundleCreateRequest LoadFromFileAsync( string path, uint crc, ulong offset );

      public static AssetBundleCreateRequest LoadFromFileAsync( string path, uint crc )
      {
         ulong offset = 0uL;
         return LoadFromFileAsync( path, crc, offset );
      }

      public static AssetBundleCreateRequest LoadFromFileAsync( string path )
      {
         ulong offset = 0uL;
         uint crc = 0u;
         return LoadFromFileAsync( path, crc, offset );
      }

      public static extern AssetBundle LoadFromFile( string path, uint crc, ulong offset );

      public static AssetBundle LoadFromFile( string path, uint crc )
      {
         ulong offset = 0uL;
         return LoadFromFile( path, crc, offset );
      }

      public static AssetBundle LoadFromFile( string path )
      {
         ulong offset = 0uL;
         uint crc = 0u;
         return LoadFromFile( path, crc, offset );
      }

      public static extern AssetBundleCreateRequest LoadFromMemoryAsync( byte[] binary, uint crc );

      public static AssetBundleCreateRequest LoadFromMemoryAsync( byte[] binary )
      {
         uint crc = 0u;
         return LoadFromMemoryAsync( binary, crc );
      }

#if IL2CPP
      public static extern AssetBundle LoadFromMemory( Il2CppStructArray<byte> binary, uint crc );
#else
      public static extern AssetBundle LoadFromMemory( byte[] binary, uint crc );
#endif

      public static AssetBundle LoadFromMemory( byte[] binary )
      {
         uint crc = 0u;
         return LoadFromMemory( binary, crc );
      }

      public extern bool Contains( string name );

      [Obsolete( "Method Load has been deprecated. Script updater cannot update it as the loading behaviour has changed. Please use LoadAsset instead and check the documentation for details.", true )]
      public Object Load( string name )
      {
         return null;
      }

      [Obsolete( "Method Load has been deprecated. Script updater cannot update it as the loading behaviour has changed. Please use LoadAsset instead and check the documentation for details.", true )]
      public T Load<T>( string name ) where T : Object
      {
         return (T)null;
      }

      public extern Object Load( string name, Type type );

      public extern AssetBundleRequest LoadAsync( string name, Type type );

      public extern Object[] LoadAll( Type type );

      [Obsolete( "Method LoadAll has been deprecated. Script updater cannot update it as the loading behaviour has changed. Please use LoadAllAssets instead and check the documentation for details.", true )]
      public Object[] LoadAll()
      {
         return null;
      }

      [Obsolete( "Method LoadAll has been deprecated. Script updater cannot update it as the loading behaviour has changed. Please use LoadAllAssets instead and check the documentation for details.", true )]
      public T[] LoadAll<T>() where T : Object
      {
         return null;
      }

      public Object LoadAsset( string name )
      {
         return LoadAsset( name, typeof( Object ) );
      }

      public T LoadAsset<T>( string name ) where T : Object
      {
         return (T)LoadAsset( name, typeof( T ) );
      }

      public Object LoadAsset( string name, Type type )
      {
         if( name == null )
         {
            throw new NullReferenceException( "The input asset name cannot be null." );
         }

         if( name.Length == 0 )
         {
            throw new ArgumentException( "The input asset name cannot be empty." );
         }

         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAsset_Internal( name, type );
      }

      private extern Object LoadAsset_Internal( string name, Type type );

      public AssetBundleRequest LoadAssetAsync( string name )
      {
         return LoadAssetAsync( name, typeof( Object ) );
      }

      public AssetBundleRequest LoadAssetAsync<T>( string name )
      {
         return LoadAssetAsync( name, typeof( T ) );
      }

      public AssetBundleRequest LoadAssetAsync( string name, Type type )
      {
         if( name == null )
         {
            throw new NullReferenceException( "The input asset name cannot be null." );
         }

         if( name.Length == 0 )
         {
            throw new ArgumentException( "The input asset name cannot be empty." );
         }

         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAssetAsync_Internal( name, type );
      }

      private extern AssetBundleRequest LoadAssetAsync_Internal( string name, Type type );

      public Object[] LoadAssetWithSubAssets( string name )
      {
         return LoadAssetWithSubAssets( name, typeof( Object ) );
      }

      public T[] LoadAssetWithSubAssets<T>( string name ) where T : Object
      {
         return Resources.ConvertObjects<T>( LoadAssetWithSubAssets( name, typeof( T ) ) );
      }

      public Object[] LoadAssetWithSubAssets( string name, Type type )
      {
         if( name == null )
         {
            throw new NullReferenceException( "The input asset name cannot be null." );
         }

         if( name.Length == 0 )
         {
            throw new ArgumentException( "The input asset name cannot be empty." );
         }

         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAssetWithSubAssets_Internal( name, type );
      }

      internal extern Object[] LoadAssetWithSubAssets_Internal( string name, Type type );

      public AssetBundleRequest LoadAssetWithSubAssetsAsync( string name )
      {
         return LoadAssetWithSubAssetsAsync( name, typeof( Object ) );
      }

      public AssetBundleRequest LoadAssetWithSubAssetsAsync<T>( string name )
      {
         return LoadAssetWithSubAssetsAsync( name, typeof( T ) );
      }

      public AssetBundleRequest LoadAssetWithSubAssetsAsync( string name, Type type )
      {
         if( name == null )
         {
            throw new NullReferenceException( "The input asset name cannot be null." );
         }

         if( name.Length == 0 )
         {
            throw new ArgumentException( "The input asset name cannot be empty." );
         }

         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAssetWithSubAssetsAsync_Internal( name, type );
      }

      private extern AssetBundleRequest LoadAssetWithSubAssetsAsync_Internal( string name, Type type );

      public Object[] LoadAllAssets()
      {
         return LoadAllAssets( typeof( Object ) );
      }

      public T[] LoadAllAssets<T>() where T : Object
      {
         return Resources.ConvertObjects<T>( LoadAllAssets( typeof( T ) ) );
      }

      public Object[] LoadAllAssets( Type type )
      {
         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAssetWithSubAssets_Internal( "", type );
      }

      public AssetBundleRequest LoadAllAssetsAsync()
      {
         return LoadAllAssetsAsync( typeof( Object ) );
      }

      public AssetBundleRequest LoadAllAssetsAsync<T>()
      {
         return LoadAllAssetsAsync( typeof( T ) );
      }

      public AssetBundleRequest LoadAllAssetsAsync( Type type )
      {
         if( type == null )
         {
            throw new NullReferenceException( "The input type cannot be null." );
         }

         return LoadAssetWithSubAssetsAsync_Internal( "", type );
      }

      public extern void Unload( bool unloadAllLoadedObjects );

      [Obsolete( "This method is deprecated. Use GetAllAssetNames() instead." )]
      public string[] AllAssetNames()
      {
         return GetAllAssetNames();
      }

      public extern string[] GetAllAssetNames();

      public extern string[] GetAllScenePaths();
   }
}
