using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks
{
   internal static class ResourceAndAssetHooks
   {
      public static readonly Type[] All = new[]
      {
         typeof( AssetBundle_LoadFromFileAsync_Hook ),
         typeof( AssetBundle_LoadFromFile_Hook ),
         //typeof( AssetBundle_LoadFromMemoryAsync_Hook ),
         //typeof( AssetBundle_LoadFromMemory_Hook ), // Cannot be hooked! Missing path
         typeof( AssetBundle_mainAsset_Hook ),
         typeof( AssetBundle_Load_Hook ),
         typeof( AssetBundle_LoadAsync_Hook ),
         typeof( AssetBundle_LoadAll_Hook ),
         typeof( AssetBundle_LoadAsset_Internal_Hook ),
         typeof( AssetBundle_LoadAssetAsync_Internal_Hook ),
         typeof( AssetBundle_LoadAssetWithSubAssets_Internal_Hook ),
         typeof( AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Hook ),
         typeof( AssetBundleRequest_asset_Hook ),
         typeof( AssetBundleRequest_allAssets_Hook ),

         typeof( Resources_Load_Hook ),
         typeof( Resources_LoadAll_Hook ),
         //typeof( Resources_LoadAsync_Hook ), // not needed
         typeof( Resources_GetBuiltinResource_Hook ),
         //typeof( Resources_FindObjectsOfTypeAll_Hook ), // impossible
      };
   }

   internal static class AssetBundle_LoadFromFileAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFileAsync", typeof( string ), typeof( uint ), typeof( ulong ) );
      }

      delegate AssetBundleCreateRequest OriginalMethod( string path, uint crc, ulong offset );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( string path, uint crc, ulong offset )
      {
         AssetBundleCreateRequest result;

         ResourceRedirection.Hook_AssetBundleLoading_Postfix( path, out result );

         if( result == null )
         {
            result = _original( path, crc, offset );
         }

         return result;
      }
   }

   internal static class AssetBundle_LoadFromFile_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFile", typeof( string ), typeof( uint ), typeof( ulong ) );
      }

      delegate AssetBundle OriginalMethod( string path, uint crc, ulong offset );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( string path, uint crc, ulong offset )
      {
         AssetBundle result;

         ResourceRedirection.Hook_AssetBundleLoaded_Postfix( path, out result );

         if( result == null )
         {
            result = _original( path, crc, offset );
         }

         return result;
      }
   }

   //internal static class AssetBundle_LoadFromMemoryAsync_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemoryAsync", typeof( byte[] ), typeof( uint ) );
   //   }

   //   delegate AssetBundleCreateRequest OriginalMethod( string path, uint crc );

   //   static OriginalMethod _original;

   //   static void MM_Init( object detour )
   //   {
   //      _original = detour.GenerateTrampolineEx<OriginalMethod>();
   //   }

   //   static AssetBundleCreateRequest MM_Detour( byte[] binary, uint crc )
   //   {
   //      var result = _original( null, crc );

   //      ResourceRedirectionManager.Hook_AssetBundleLoading_Postfix( null, result );

   //      return result;
   //   }
   //}

   //internal static class AssetBundle_LoadFromMemory_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemory", typeof( byte[] ), typeof( uint ) );
   //   }

   //   delegate AssetBundle OriginalMethod( byte[] binary, uint crc );

   //   static OriginalMethod _original;

   //   static void MM_Init( object detour )
   //   {
   //      _original = detour.GenerateTrampolineEx<OriginalMethod>();
   //   }

   //   static AssetBundle MM_Detour( byte[] binary, uint crc )
   //   {
   //      var result = _original( binary, crc );

   //      ResourceRedirectionManager.Hook_AssetBundleLoaded_Postfix( null, result, null );

   //      return result;
   //   }
   //}


   internal static class AssetBundle_mainAsset_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AssetBundle ), "mainAsset" ).GetGetMethod();
      }

      delegate UnityEngine.Object OriginalMethod( AssetBundle self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle self )
      {
         var result = _original( self );

         ResourceRedirection.Hook_AssetLoaded_Postfix( self, null, ref result );

         return result;
      }
   }

   internal static class AssetBundle_Load_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "Load", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         ResourceRedirection.Hook_AssetLoaded_Postfix( self, null, ref result );

         return result;
      }
   }

   internal static class AssetBundle_LoadAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAsync", typeof( string ), typeof( Type ) );
      }

      delegate AssetBundleRequest OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         ResourceRedirection.Hook_AssetLoading( self, result );

         return result;
      }
   }

   internal static class AssetBundle_LoadAll_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAll", typeof( Type ) );
      }

      delegate UnityEngine.Object[] OriginalMethod( AssetBundle self, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundle self, Type type )
      {
         var result = _original( self, type );

         if( result != null )
         {
            for( int i = 0; i < result.Length; i++ )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( self, null, ref result[ i ] );
            }
         }

         return result;
      }
   }

   internal static class AssetBundle_LoadAsset_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAsset_Internal", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         ResourceRedirection.Hook_AssetLoaded_Postfix( self, null, ref result );

         return result;
      }
   }

   internal static class AssetBundle_LoadAssetAsync_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAssetAsync_Internal", typeof( string ), typeof( Type ) );
      }

      delegate AssetBundleRequest OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         ResourceRedirection.Hook_AssetLoading( self, result );

         return result;
      }
   }

   internal static class AssetBundle_LoadAssetWithSubAssets_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAssetWithSubAssets_Internal", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object[] OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         if( result != null )
         {
            for( int i = 0; i < result.Length; i++ )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( self, null, ref result[ i ] );
            }
         }

         return result;
      }
   }

   internal static class AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadAssetWithSubAssetsAsync_Internal", typeof( string ), typeof( Type ) );
      }

      delegate AssetBundleRequest OriginalMethod( AssetBundle self, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle self, string name, Type type )
      {
         var result = _original( self, name, type );

         ResourceRedirection.Hook_AssetLoading( self, result );

         return result;
      }
   }

   internal static class AssetBundleRequest_asset_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AssetBundleRequest ), "asset" ).GetGetMethod();
      }

      delegate UnityEngine.Object OriginalMethod( AssetBundleRequest self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundleRequest self )
      {
         var result = _original( self );

         ResourceRedirection.Hook_AssetLoaded_Postfix( null, self, ref result );

         return result;
      }
   }

   internal static class AssetBundleRequest_allAssets_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AssetBundleRequest ), "allAssets" ).GetGetMethod();
      }

      delegate UnityEngine.Object[] OriginalMethod( AssetBundleRequest self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundleRequest self )
      {
         var result = _original( self );

         if( result != null )
         {
            for( int i = 0; i < result.Length; i++ )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( null, self, ref result[ i ] );
            }
         }

         return result;
      }
   }

   internal static class Resources_Load_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "Load", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( string path, Type systemTypeInstance )
      {
         var result = _original( path, systemTypeInstance );

         ResourceRedirection.Hook_ResourceLoaded( path, true, ref result );

         return result;
      }
   }

   internal static class Resources_LoadAll_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "LoadAll", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object[] OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( string path, Type systemTypeInstance )
      {
         var result = _original( path, systemTypeInstance );

         if( result != null )
         {
            for( int i = 0; i < result.Length; i++ )
            {
               ResourceRedirection.Hook_ResourceLoaded( path, false, ref result[ i ] );
            }
         }

         return result;
      }
   }

   internal static class Resources_GetBuiltinResource_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "GetBuiltinResource", typeof( string ), typeof( Type ) );
      }

      delegate UnityEngine.Object OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( string path, Type systemTypeInstance )
      {
         var result = _original( path, systemTypeInstance );

         ResourceRedirection.Hook_ResourceLoaded( path, true, ref result );

         return result;
      }
   }

   // PROBLEM: No way to obtain full path!
   //internal static class Resources_FindObjectsOfTypeAll_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Method( typeof( Resources ), "FindObjectsOfTypeAll", typeof( Type ) );
   //   }

   //   delegate UnityEngine.Object[] OriginalMethod( Type systemTypeInstance );

   //   static OriginalMethod _original;

   //   static void MM_Init( object detour )
   //   {
   //      _original = detour.GenerateTrampolineEx<OriginalMethod>();
   //   }

   //   static UnityEngine.Object[] MM_Detour( Type systemTypeInstance )
   //   {
   //      IEnumerable result = _original( systemTypeInstance );

   //      List<UnityEngine.Object> actualResult = new List<UnityEngine.Object>();
   //      if( result != null )
   //      {
   //         foreach( UnityEngine.Object item in result )
   //         {
   //            var obj = item;
   //            ResourceRedirectionManager.Hook_ResourceLoaded( null, null, ref obj );
   //            actualResult.Add( obj );
   //         }
   //      }

   //      return actualResult.ToArray();
   //   }
   //}
}
