#if IL2CPP

using System;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace UnityEngine
{
   internal class AssetBundleProxy
   {
      private delegate IntPtr CreateDynamicFontFromOSFont_InternalDelegate( IntPtr fontname, int size );
      private static readonly CreateDynamicFontFromOSFont_InternalDelegate CreateDynamicFontFromOSFont_InternalDelegateField;

      public static Font CreateDynamicFontFromOSFont( string fontname, int size )
      {
         if( string.IsNullOrEmpty( fontname ) )
            throw new System.ArgumentNullException( nameof( fontname ), "The path cannot be null or empty." );

         IntPtr ptr = CreateDynamicFontFromOSFont_InternalDelegateField( IL2CPP.ManagedStringToIl2Cpp( fontname ), size );
         return ptr != IntPtr.Zero ? new Font( ptr ) : null;
      }

      private delegate IntPtr LoadFromFile_InternalDelegate( IntPtr path, uint crc, ulong offset );
      private static readonly LoadFromFile_InternalDelegate LoadFromFile_InternalDelegateField;
      private delegate IntPtr LoadAssetWithSubAssets_InternalDelegate( IntPtr _this, IntPtr name, IntPtr type );
      private static readonly LoadAssetWithSubAssets_InternalDelegate LoadAssetWithSubAssets_InternalDelegateField;

      public static AssetBundleProxy LoadFromFile( string path ) => LoadFromFile( path, 0u, 0UL );

      public static AssetBundleProxy LoadFromFile( string path, uint crc ) => LoadFromFile( path, crc, 0UL );

      public static AssetBundleProxy LoadFromFile( string path, uint crc, ulong offset )
      {
         if( string.IsNullOrEmpty( path ) )
            throw new System.ArgumentException( nameof( path ), "The path cannot be null or empty." );

         IntPtr ptr = LoadFromFile_InternalDelegateField( IL2CPP.ManagedStringToIl2Cpp( path ), crc, offset );
         return ptr != IntPtr.Zero ? new AssetBundleProxy( ptr ) : null;
      }

      private IntPtr _instancePointer = IntPtr.Zero;

      public AssetBundleProxy( IntPtr ptr ) { _instancePointer = ptr; }

      static AssetBundleProxy()
      {
         LoadFromFile_InternalDelegateField = IL2CPP.ResolveICall<LoadFromFile_InternalDelegate>( "UnityEngine.AssetBundle::LoadFromFile_Internal(System.String,System.UInt32,System.UInt64)" );
         LoadAssetWithSubAssets_InternalDelegateField = IL2CPP.ResolveICall<LoadAssetWithSubAssets_InternalDelegate>( "UnityEngine.AssetBundle::LoadAssetWithSubAssets_Internal" );
         CreateDynamicFontFromOSFont_InternalDelegateField = IL2CPP.ResolveICall<CreateDynamicFontFromOSFont_InternalDelegate>( "UnityEngine.Font::CreateDynamicFontFromOSFont(System.String,System.Int32)" );
      }

      public Il2CppReferenceArray<Object> LoadAll() => LoadAllAssets();

      public Il2CppReferenceArray<Object> LoadAllAssets() => LoadAllAssets<Object>();

      public Il2CppReferenceArray<T> LoadAll<T>() where T : Object => LoadAllAssets<T>();

      public Il2CppReferenceArray<T> LoadAllAssets<T>() where T : Object
      {
         IntPtr ptr = LoadAllAssets( Il2CppType.Of<T>().Pointer );
         return ptr != IntPtr.Zero ? new Il2CppReferenceArray<T>( ptr ) : null;
      }

      public Il2CppReferenceArray<Object> LoadAll( Il2CppSystem.Type type ) => LoadAllAssets( type );

      public Il2CppReferenceArray<Object> LoadAllAssets( Il2CppSystem.Type type )
      {
         if( type == null )
            throw new ArgumentNullException( nameof( type ) );

         IntPtr ptr = LoadAllAssets( type.Pointer );
         return ptr != IntPtr.Zero ? new Il2CppReferenceArray<Object>( ptr ) : null;
      }

      public IntPtr LoadAll( IntPtr typePointer ) => LoadAllAssets( typePointer );

      public IntPtr LoadAllAssets( IntPtr typePointer ) => LoadAssetWithSubAssets( string.Empty, typePointer );

      public Il2CppReferenceArray<Object> LoadAssetWithSubAssets( string name ) => LoadAssetWithSubAssets<Object>( name );

      public Il2CppReferenceArray<T> LoadAssetWithSubAssets<T>( string name ) where T : Object
      {
         IntPtr ptr = LoadAssetWithSubAssets( name, Il2CppType.Of<T>().Pointer );

         return ptr != IntPtr.Zero ? new Il2CppReferenceArray<T>( ptr ) : null;
      }

      public Il2CppReferenceArray<Object> LoadAssetWithSubAssets( string name, Il2CppSystem.Type type )
      {
         if( type == null )
            throw new ArgumentNullException( nameof( type ) );

         IntPtr ptr = LoadAssetWithSubAssets( name, type.Pointer );
         return ptr != IntPtr.Zero ? new Il2CppReferenceArray<Object>( ptr ) : null;
      }

      public IntPtr LoadAssetWithSubAssets( string name, IntPtr typePointer )
      {
         if( _instancePointer == IntPtr.Zero )
            throw new NullReferenceException();

         if( typePointer == IntPtr.Zero )
            throw new ArgumentNullException( nameof( typePointer ) );

         return LoadAssetWithSubAssets_InternalDelegateField( _instancePointer, IL2CPP.ManagedStringToIl2Cpp( name ), typePointer );
      }
   }
}

#endif
