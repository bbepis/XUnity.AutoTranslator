using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector.Hooks
{
   internal static class ResourceAndAssetHooks
   {
      public static readonly Type[] GeneralHooks = new[]
      {
         typeof( AssetBundle_LoadFromFileAsync_Hook ),
         typeof( AssetBundle_LoadFromFile_Hook ),
         typeof( AssetBundle_LoadFromMemoryAsync_Hook ),
         typeof( AssetBundle_LoadFromMemory_Hook ),
         typeof( AssetBundle_LoadFromStreamAsync_Hook ),
         typeof( AssetBundle_LoadFromStream_Hook ),

         typeof( AssetBundle_mainAsset_Hook ),
         typeof( AssetBundle_returnMainAsset_Hook ),
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
         typeof( Resources_GetBuiltinResource_Old_Hook ),
         typeof( Resources_GetBuiltinResource_New_Hook ),
         //typeof( Resources_FindObjectsOfTypeAll_Hook ), // impossible
         

         //typeof( Resources_UnloadAsset_Hook )
      };

      public static readonly Type[] SyncOverAsyncHooks = new Type[]
      {
         typeof( AssetBundleCreateRequest_assetBundle_Hook ),
         typeof( AssetBundleCreateRequest_DisableCompatibilityChecks_Hook ),
         typeof( AssetBundleCreateRequest_SetEnableCompatibilityChecks_Hook ),

         typeof( AsyncOperation_isDone_Hook ),
         typeof( AsyncOperation_progress_Hook ),
         typeof( AsyncOperation_priority_Hook ),
         typeof( AsyncOperation_set_priority_Hook ),
         typeof( AsyncOperation_allowSceneActivation_Hook ),
         typeof( AsyncOperation_set_allowSceneActivation_Hook ),
         typeof( AsyncOperation_Finalize_Hook ),
      };
   }

   internal static class AssetBundleCreateRequest_assetBundle_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( AssetBundleCreateRequest ), "assetBundle" ).GetGetMethod();
      }

      delegate AssetBundle OriginalMethod( AssetBundleCreateRequest self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( AssetBundleCreateRequest self )
      {
         if( ResourceRedirection.TryGetAssetBundle( self, out var info ) )
         {
            AssetBundle bundle;

            if( info.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle )
            {
               bundle = info.Bundle;
            }
            else
            {
               bundle = _original( self );
            }

            if( !info.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetBundleLoaded_Postfix( info.Parameters, ref bundle );
            }

            if( bundle != null && info != null ) // should only be null if loaded through non-hooked methods
            {
               var path = info.Parameters.Path;
               if( path != null )
               {
                  var ext = bundle.GetOrCreateExtensionData<AssetBundleExtensionData>();
                  ext.Path = path;
               }
            }

            return bundle;
         }
         else
         {
            return _original( self );
         }
      }
   }

   internal static class AssetBundleCreateRequest_DisableCompatibilityChecks_Hook
   {
      static bool Prepare( object instance )
      {
         var enableCompatChecks = AccessToolsShim.Method( typeof( AssetBundleCreateRequest ), "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
         return enableCompatChecks == null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundleCreateRequest ), "DisableCompatibilityChecks" );
      }

      delegate void OriginalMethod( AssetBundleCreateRequest self );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AssetBundleCreateRequest self )
      {
         if( !ResourceRedirection.TryGetAssetBundle( self, out var result ) )
         {
            _original( self );
         }
      }
   }

   internal static class AssetBundleCreateRequest_SetEnableCompatibilityChecks_Hook
   {
      static bool Prepare( object instance )
      {
         var enableCompatChecks = AccessToolsShim.Method( typeof( AssetBundleCreateRequest ), "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
         return enableCompatChecks != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundleCreateRequest ), "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
      }

      delegate void OriginalMethod( AssetBundleCreateRequest self, bool set );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AssetBundleCreateRequest self, bool set )
      {
         if( !ResourceRedirection.TryGetAssetBundle( self, out var result ) )
         {
            _original( self, set );
         }
      }
   }

   internal static class AssetBundle_LoadFromFileAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFileAsync_Internal", typeof( string ), typeof( uint ), typeof( ulong ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFileAsync", typeof( string ), typeof( uint ), typeof( ulong ) );
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

         var parameters = new AssetBundleLoadingParameters( null, path, crc, offset, null, 0, AssetBundleLoadType.LoadFromFile );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         if( !context.SkipOriginalCall )
         {
            var p = context.Parameters;
            result = _original( p.Path, p.Crc, p.Offset );
         }

         ResourceRedirection.Hook_AssetBundleLoading_Postfix( context, result );

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
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFile_Internal", typeof( string ), typeof( uint ), typeof( ulong ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromFile", typeof( string ), typeof( uint ), typeof( ulong ) );
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

         var parameters = new AssetBundleLoadingParameters( null, path, crc, offset, null, 0, AssetBundleLoadType.LoadFromFile );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( p.Path, p.Crc, p.Offset );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( parameters, ref result );
         }

         if( result != null && p.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = p.Path;
         }

         return result;
      }
   }













   internal static class AssetBundle_LoadFromMemoryAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemoryAsync_Internal", typeof( byte[] ), typeof( uint ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemoryAsync", typeof( byte[] ), typeof( uint ) );
      }

      delegate AssetBundleCreateRequest OriginalMethod( byte[] binary, uint crc );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( byte[] binary, uint crc )
      {
         AssetBundleCreateRequest result;

         var parameters = new AssetBundleLoadingParameters( binary, null, crc, 0, null, 0, AssetBundleLoadType.LoadFromMemory );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         if( !context.SkipOriginalCall )
         {
            var p = context.Parameters;
            result = _original( p.Binary, p.Crc );
         }

         ResourceRedirection.Hook_AssetBundleLoading_Postfix( context, result );

         return result;
      }
   }

   internal static class AssetBundle_LoadFromMemory_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemory_Internal", typeof( byte[] ), typeof( uint ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromMemory", typeof( byte[] ), typeof( uint ) );
      }

      delegate AssetBundle OriginalMethod( byte[] binary, uint crc );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( byte[] binary, uint crc )
      {
         AssetBundle result;

         var parameters = new AssetBundleLoadingParameters( binary, null, crc, 0, null, 0, AssetBundleLoadType.LoadFromMemory );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( p.Binary, p.Crc );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( parameters, ref result );
         }

         if( result != null && p.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = p.Path;
         }

         return result;
      }
   }













   internal static class AssetBundle_LoadFromStreamAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromStreamAsyncInternal", typeof( Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromStreamAsync", typeof( Stream ), typeof( uint ), typeof( uint ) );
      }

      delegate AssetBundleCreateRequest OriginalMethod( Stream stream, uint crc, uint managedReadBufferSize );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( Stream stream, uint crc, uint managedReadBufferSize )
      {
         AssetBundleCreateRequest result;

         var parameters = new AssetBundleLoadingParameters( null, null, crc, 0, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         if( !context.SkipOriginalCall )
         {
            var p = context.Parameters;
            result = _original( p.Stream, p.Crc, p.ManagedReadBufferSize );
         }

         ResourceRedirection.Hook_AssetBundleLoading_Postfix( context, result );

         return result;
      }
   }

   internal static class AssetBundle_LoadFromStream_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromStreamInternal", typeof( Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( typeof( AssetBundle ), "LoadFromStream", typeof( Stream ), typeof( uint ), typeof( uint ) );
      }

      delegate AssetBundle OriginalMethod( Stream stream, uint crc, uint managedReadBufferSize );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( Stream stream, uint crc, uint managedReadBufferSize )
      {
         AssetBundle result;

         var parameters = new AssetBundleLoadingParameters( null, null, crc, 0, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory );
         var context = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( p.Stream, p.Crc, p.ManagedReadBufferSize );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( parameters, ref result );
         }

         if( result != null && p.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = p.Path;
         }

         return result;
      }
   }
















   internal static class AssetBundle_mainAsset_Hook
   {
      static bool Prepare( object instance )
      {
         var returnMainAsset = AccessToolsShim.Method( typeof( AssetBundle ), "returnMainAsset", new[] { typeof( AssetBundle ) } );
         return returnMainAsset == null;
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

         var parameters = new AssetLoadingParameters( null, null, AssetLoadType.LoadMainAsset );
         var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

         if( !context.SkipOriginalCall )
         {
            result = _original( self );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
         }

         return result;
      }
   }

   internal static class AssetBundle_returnMainAsset_Hook
   {
      static bool Prepare( object instance )
      {
         var returnMainAsset = AccessToolsShim.Method( typeof( AssetBundle ), "returnMainAsset", new[] { typeof( AssetBundle ) } );
         return returnMainAsset != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( AssetBundle ), "returnMainAsset", new[] { typeof( AssetBundle ) } );
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

         var parameters = new AssetLoadingParameters( null, null, AssetLoadType.LoadMainAsset );
         var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

         if( !context.SkipOriginalCall )
         {
            result = _original( self );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
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

         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
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

         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, self, ref result );

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

         var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
         var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( self, p.Type );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
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

         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

         var p = context.Parameters;
         if( !context.SkipOriginalCall )
         {
            result = _original( self, p.Name, p.Type );
         }

         if( !context.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
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

         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, self, ref result );

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
            var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
            var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

            var p = context.Parameters;
            if( !context.SkipOriginalCall )
            {
               result = _original( self, name, p.Type );
            }

            if( !context.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
            }
         }
         else
         {
            var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamedWithSubAssets );
            var context = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, self, ref result );

            var p = context.Parameters;
            if( !context.SkipOriginalCall )
            {
               result = _original( self, p.Name, p.Type );
            }

            if( !context.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetLoaded_Postfix( context.Parameters, self, ref result );
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
            var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
            var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, self, ref result );

            var p = context.Parameters;
            if( !context.SkipOriginalCall )
            {
               result = _original( self, name, p.Type );
            }

            ResourceRedirection.Hook_AssetLoading_Postfix( context, result );
         }
         else
         {
            var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamedWithSubAssets );
            var context = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, self, ref result );

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
               ResourceRedirection.Hook_AssetLoaded_Postfix( info.Parameters, info.Bundle, ref result );
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
         return AccessToolsShim.Property( typeof( AssetBundleRequest ), "allAssets" ) != null;
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
               ResourceRedirection.Hook_AssetLoaded_Postfix( info.Parameters, info.Bundle, ref result );
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

         var parameters = new ResourceLoadedParameters( path, systemTypeInstance, ResourceLoadType.LoadNamed );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref result );

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

         var parameters = new ResourceLoadedParameters( path, systemTypeInstance, ResourceLoadType.LoadByType );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref result );

         return result;
      }
   }

   internal static class Resources_GetBuiltinResource_Old_Hook
   {
      static bool Prepare( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "GetBuiltinResource", typeof( Type ), typeof( string ) ) == null;
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

         var parameters = new ResourceLoadedParameters( path, systemTypeInstance, ResourceLoadType.LoadNamedBuiltIn );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref result );

         return result;
      }
   }

   internal static class Resources_GetBuiltinResource_New_Hook
   {
      static bool Prepare( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "GetBuiltinResource", typeof( string ), typeof( Type ) ) == null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( Resources ), "GetBuiltinResource", typeof( Type ), typeof( string ) );
      }

      delegate UnityEngine.Object OriginalMethod( Type systemTypeInstance, string path );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( Type systemTypeInstance, string path )
      {
         var result = _original( systemTypeInstance, path );

         var parameters = new ResourceLoadedParameters( path, systemTypeInstance, ResourceLoadType.LoadNamedBuiltIn );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref result );

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
