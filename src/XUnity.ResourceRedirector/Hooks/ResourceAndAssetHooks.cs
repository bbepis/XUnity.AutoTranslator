using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks
{
   internal static class ResourceAndAssetHooks
   {
      public static readonly Type[] GeneralHooks = new[]
      {
         //typeof( AssetBundleCreateRequest_assetBundle_Hook ),
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
         

         //typeof( Resources_UnloadAsset_Hook )
      };

      public static readonly Type[] SyncOverAsyncHooks = new Type[]
      {
         typeof( AsyncOperation_isDone_Hook ),
         typeof( AsyncOperation_progress_Hook ),
         typeof( AsyncOperation_priority_Hook ),
         typeof( AsyncOperation_set_priority_Hook ),
         typeof( AsyncOperation_allowSceneActivation_Hook ),
         typeof( AsyncOperation_set_allowSceneActivation_Hook ),
         typeof( AsyncOperation_Finalize_Hook ),
      };
   }

   //internal static class AssetBundleCreateRequest_assetBundle_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Property( typeof( AssetBundleCreateRequest ), "assetBundle" ).GetGetMethod();
   //   }

   //   delegate AssetBundle OriginalMethod( AssetBundleCreateRequest self );

   //   static OriginalMethod _original;

   //   static void MM_Init( object detour )
   //   {
   //      _original = detour.GenerateTrampolineEx<OriginalMethod>();
   //   }

   //   static AssetBundle MM_Detour( AssetBundleCreateRequest self )
   //   {
   //      var result = _original( self );

   //      ResourceRedirection.AssociateAssetBundleWithDummy( self, result );

   //      return result;
   //   }
   //}

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

         var intention = ResourceRedirection.Hook_AssetBundleLoading_Prefix( path, crc, offset, AssetBundleLoadType.LoadFromFile, out result );

         if( !intention.SkipOriginalCall )
         {
            var p = intention.Parameters;
            result = _original( p.Path, p.Crc, p.Offset );
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

         var intention = ResourceRedirection.Hook_AssetBundleLoading_Prefix( path, crc, offset, AssetBundleLoadType.LoadFromFile, out result );

         if( !intention.SkipOriginalCall )
         {
            var p = intention.Parameters;
            result = _original( p.Path, p.Crc, p.Offset );
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
         UnityEngine.Object result = null;

         var intention = ResourceRedirection.Hook_AssetLoading_Prefix( null, null, AssetLoadType.LoadMainAsset, self, ref result );

         if( !intention.SkipOriginalCall )
         {
            result = _original( self );
         }

         if( !intention.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( null, null, AssetLoadType.LoadMainAsset, self, null, ref result );
         }

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
         UnityEngine.Object result = null;

         var intention = ResourceRedirection.Hook_AssetLoading_Prefix( name, type, AssetLoadType.LoadNamed, self, ref result );

         var p = intention.Parameters;
         if( !intention.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         if( !intention.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( p.Name, p.Type, AssetLoadType.LoadNamed, self, null, ref result );
         }

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
         AssetBundleRequest result = null;

         var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( name, type, AssetLoadType.LoadNamed, self, ref result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         ResourceRedirection.Hook_AssetLoading_Postfix( context, result );

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
         UnityEngine.Object[] result = null;

         var intention = ResourceRedirection.Hook_AssetLoading_Prefix( null, type, AssetLoadType.LoadByType, self, ref result );

         var p = intention.Parameters;
         if( !intention.SkipOriginalCall )
         {
            result = _original( self, p.Type );
         }

         if( !intention.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( null, p.Type, AssetLoadType.LoadByType, self, null, ref result );
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
         UnityEngine.Object result = null;

         var intention = ResourceRedirection.Hook_AssetLoading_Prefix( name, type, AssetLoadType.LoadNamed, self, ref result );

         var p = intention.Parameters;
         if( !intention.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         if( !intention.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( p.Name, p.Type, AssetLoadType.LoadNamed, self, null, ref result );
         }

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
         AssetBundleRequest result = null;

         var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( name, type, AssetLoadType.LoadNamed, self, ref result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         ResourceRedirection.Hook_AssetLoading_Postfix( context, result );

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
         UnityEngine.Object[] result = null;

         if( name == string.Empty )
         {
            var intention = ResourceRedirection.Hook_AssetLoading_Prefix( null, type, AssetLoadType.LoadByType, self, ref result );

            var p = intention.Parameters;
            if( !intention.SkipOriginalCall )
            {
               result = _original( self, name, p.Type );
            }

            if( !intention.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( null, p.Type, AssetLoadType.LoadByType, self, null, ref result );
            }
         }
         else
         {
            var intention = ResourceRedirection.Hook_AssetLoading_Prefix( name, type, AssetLoadType.LoadNamedWithSubAssets, self, ref result );

            var p = intention.Parameters;
            if( !intention.SkipOriginalCall )
            {
               result = _original( self, p.Name, p.Type );
            }

            if( !intention.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( p.Name, p.Type, AssetLoadType.LoadNamedWithSubAssets, self, null, ref result );
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
         AssetBundleRequest result = null;

         if( name == string.Empty )
         {
            var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( null, type, AssetLoadType.LoadByType, self, ref result );

            var p = context.Parameters;
            if( !context.SkipOriginalCall )
            {
               result = _original( self, name, p.Type );
            }

            ResourceRedirection.Hook_AssetLoading_Postfix( context, result );
         }
         else
         {
            var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( name, type, AssetLoadType.LoadNamedWithSubAssets, self, ref result );

            var p = context.Parameters;
            if( !context.SkipOriginalCall )
            {
               result = _original( self, p.Name, p.Type );
            }

            ResourceRedirection.Hook_AssetLoading_Postfix( context, result );
         }

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
         if( ResourceRedirection.TryGetAssetBundleLoadInfo( self, out var info ) )
         {
            UnityEngine.Object result = null;

            if( info.ResolveType == AsyncAssetLoadingResolve.ThroughAssets )
            {
               var assets = info.Assets;
               if( assets != null && assets.Length > 0 )
               {
                  result = assets[ 0 ];
               }
            }
            else
            {
               result = _original( self );
            }

            if( !info.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( null, null, 0, null, self, ref result );
            }

            return result;
         }
         else
         {
            return _original( self );
         }
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
         if( ResourceRedirection.TryGetAssetBundleLoadInfo( self, out var info ) )
         {
            UnityEngine.Object[] result;

            if( info.ResolveType == AsyncAssetLoadingResolve.ThroughAssets )
            {
               result = info.Assets;
            }
            else
            {
               result = _original( self );
            }

            if( !info.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( null, null, 0, null, self, ref result );
            }

            return result;
         }
         else
         {
            return _original( self );
         }
      }
   }

   //internal static class Resources_UnloadAsset_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Method( typeof( Resources ), "UnloadAsset", typeof( UnityEngine.Object ) );
   //   }

   //   delegate void OriginalMethod( UnityEngine.Object assetToUnload );

   //   static OriginalMethod _original;

   //   static void MM_Init( object detour )
   //   {
   //      _original = detour.GenerateTrampolineEx<OriginalMethod>();
   //   }

   //   static void MM_Detour( UnityEngine.Object assetToUnload )
   //   {
   //      // nothing to do!

   //      XuaLogger.ResourceRedirector.Warn( "Unloading asset: " + assetToUnload.GetType().Name + " (" + assetToUnload.name + ")" );

   //      _original( assetToUnload );
   //   }
   //}

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

         ResourceRedirection.Hook_ResourceLoaded_Postfix( path, systemTypeInstance, ResourceLoadType.LoadNamed, ref result );

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

         ResourceRedirection.Hook_ResourceLoaded_Postfix( path, systemTypeInstance, ResourceLoadType.LoadByType, ref result );

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

         ResourceRedirection.Hook_ResourceLoaded_Postfix( path, systemTypeInstance, ResourceLoadType.LoadNamedBuiltIn, ref result );

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

   internal static class AsyncOperation_isDone_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "isDone" ).GetGetMethod();
      }

      delegate bool OriginalMethod( AsyncOperation self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( AsyncOperation self )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return true;
         }

         return _original( self );
      }
   }

   internal static class AsyncOperation_progress_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "progress" ).GetGetMethod();
      }

      delegate float OriginalMethod( AsyncOperation self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static float MM_Detour( AsyncOperation self )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return 100.0f;
         }

         return _original( self );
      }
   }

   internal static class AsyncOperation_priority_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "priority" ).GetGetMethod();
      }

      delegate int OriginalMethod( AsyncOperation self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static int MM_Detour( AsyncOperation self )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return 0;
         }

         return _original( self );
      }
   }

   internal static class AsyncOperation_set_priority_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "priority" ).GetSetMethod();
      }

      delegate void OriginalMethod( AsyncOperation self, int value );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation self, int value )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return;
         }

         _original( self, value );
      }
   }

   internal static class AsyncOperation_allowSceneActivation_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "allowSceneActivation" ).GetGetMethod();
      }

      delegate bool OriginalMethod( AsyncOperation self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( AsyncOperation self )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return true; // do not believe this has an impact
         }

         return _original( self );
      }
   }

   internal static class AsyncOperation_set_allowSceneActivation_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AsyncOperation ), "allowSceneActivation" ).GetSetMethod();
      }

      delegate void OriginalMethod( AsyncOperation self, bool value );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation self, bool value )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return;
         }

         _original( self, value );
      }
   }

   internal static class AsyncOperation_Finalize_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return typeof( AsyncOperation ).GetMethod( "Finalize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
      }

      delegate void OriginalMethod( AsyncOperation self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation self )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( self ) )
         {
            return;
         }

         _original( self );
      }
   }
}
