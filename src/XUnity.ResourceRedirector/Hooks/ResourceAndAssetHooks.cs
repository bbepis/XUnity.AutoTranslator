using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
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
         typeof( Resources_GetBuiltinResource_Old_Hook ),
         typeof( Resources_GetBuiltinResource_New_Hook ),
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
         return UnityTypes.AssetBundleCreateRequest != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AssetBundleCreateRequest?.ClrType, "assetBundle" )?.GetGetMethod();
      }

      static bool Prefix( AssetBundleCreateRequest __instance, ref AssetBundle __result, ref AsyncAssetBundleLoadInfo __state )
      {
         if( ResourceRedirection.TryGetAssetBundle( __instance, out __state ) )
         {
            if( __state.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle )
            {
               __result = __state.Bundle;
               return false;
            }
            else
            {
               return true;
            }
         }
         else
         {
            return true;
         }
      }

      static void Postfix( ref AssetBundle __result, ref AsyncAssetBundleLoadInfo __state )
      {
         if( __state != null )
         {
            if( !__state.SkipAllPostfixes )
            {
               ResourceRedirection.Hook_AssetBundleLoaded_Postfix( __state.Parameters, ref __result );
            }

            if( __result != null && __state != null ) // should only be null if loaded through non-hooked methods
            {
               var path = __state.Parameters.Path;
               if( path != null )
               {
                  var ext = __result.GetOrCreateExtensionData<AssetBundleExtensionData>();
                  ext.Path = path;
               }
            }
         }
      }

#if MANAGED
      delegate AssetBundle OriginalMethod( AssetBundleCreateRequest __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( AssetBundleCreateRequest __instance )
      {
         AssetBundle __result = null;
         AsyncAssetBundleLoadInfo __state = null;
         var callOriginal = Prefix( __instance, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundleCreateRequest_DisableCompatibilityChecks_Hook
   {
      static bool Prepare( object instance )
      {
         var enableCompatChecks = AccessToolsShim.Method( UnityTypes.AssetBundleCreateRequest?.ClrType, "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
         return enableCompatChecks == null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AssetBundleCreateRequest?.ClrType, "DisableCompatibilityChecks" );
      }

      static bool Prefix( AssetBundleCreateRequest __instance )
      {
         return !ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance );
      }

#if MANAGED
      delegate void OriginalMethod( AssetBundleCreateRequest __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AssetBundleCreateRequest __instance )
      {
         if( Prefix( __instance ) )
         {
            _original( __instance );
         }
      }
#endif
   }

   internal static class AssetBundleCreateRequest_SetEnableCompatibilityChecks_Hook
   {
      static bool Prepare( object instance )
      {
         var enableCompatChecks = AccessToolsShim.Method( UnityTypes.AssetBundleCreateRequest?.ClrType, "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
         return enableCompatChecks != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AssetBundleCreateRequest?.ClrType, "SetEnableCompatibilityChecks", new[] { typeof( bool ) } );
      }

      static bool Prefix( AssetBundleCreateRequest __instance, bool set )
      {
         return !ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance );
      }

#if MANAGED
      delegate void OriginalMethod( AssetBundleCreateRequest __instance, bool set );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AssetBundleCreateRequest __instance, bool set )
      {
         if( Prefix( __instance, set ) )
         {
            _original( __instance, set );
         }
      }
#endif
   }

   internal static class AssetBundle_LoadFromFileAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromFileAsync_Internal", typeof( string ), typeof( uint ), typeof( ulong ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromFileAsync", typeof( string ), typeof( uint ), typeof( ulong ) );
      }

      static bool Prefix( ref string path, ref uint crc, ref ulong offset, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
      {
         var parameters = new AssetBundleLoadingParameters( null, path, crc, offset, null, 0, AssetBundleLoadType.LoadFromFile );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         path = p.Path;
         crc = p.Crc;
         offset = p.Offset;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetBundleLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleCreateRequest OriginalMethod( string path, uint crc, ulong offset );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( string path, uint crc, ulong offset )
      {
         AssetBundleCreateRequest __result = null;
         AsyncAssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref path, ref crc, ref offset, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( path, crc, offset );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadFromFile_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromFile_Internal", typeof( string ), typeof( uint ), typeof( ulong ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromFile", typeof( string ), typeof( uint ), typeof( ulong ) );
      }

      static bool Prefix( ref string path, ref uint crc, ref ulong offset, ref AssetBundle __result, ref AssetBundleLoadingContext __state )
      {
         var parameters = new AssetBundleLoadingParameters( null, path, crc, offset, null, 0, AssetBundleLoadType.LoadFromFile );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         path = p.Path;
         crc = p.Crc;
         offset = p.Offset;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundle __result, ref AssetBundleLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( __state.Parameters, ref __result );
         }

         if( __result != null && __state.Parameters.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = __result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = __state.Parameters.Path;
         }
      }

#if MANAGED
      delegate AssetBundle OriginalMethod( string path, uint crc, ulong offset );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( string path, uint crc, ulong offset )
      {
         AssetBundle __result = null;
         AssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref path, ref crc, ref offset, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( path, crc, offset );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }













   internal static class AssetBundle_LoadFromMemoryAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync_Internal", typeof( byte[] ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync", typeof( byte[] ), typeof( uint ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync_Internal", typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemoryAsync", typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> ), typeof( uint ) );
#endif
      }

#if MANAGED
      static bool Prefix( ref byte[] binary, ref uint crc, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
#else
      static bool Prefix( ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> binary, ref uint crc, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
#endif
      {
         var parameters = new AssetBundleLoadingParameters( binary, null, crc, 0, null, 0, AssetBundleLoadType.LoadFromMemory );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         binary = p.Binary;
         crc = p.Crc;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetBundleLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleCreateRequest OriginalMethod( byte[] binary, uint crc );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( byte[] binary, uint crc )
      {
         AssetBundleCreateRequest __result = null;
         AsyncAssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref binary, ref crc, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( binary, crc );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadFromMemory_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemory_Internal", typeof( byte[] ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemory", typeof( byte[] ), typeof( uint ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemory_Internal", typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromMemory", typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> ), typeof( uint ) );
#endif
      }

#if MANAGED
      static bool Prefix( ref byte[] binary, ref uint crc, ref AssetBundle __result, ref AssetBundleLoadingContext __state )
#else
      static bool Prefix( ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> binary, ref uint crc, ref AssetBundle __result, ref AssetBundleLoadingContext __state )
#endif
      {
         var parameters = new AssetBundleLoadingParameters( binary, null, crc, 0, null, 0, AssetBundleLoadType.LoadFromMemory );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         binary = p.Binary;
         crc = p.Crc;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundle __result, ref AssetBundleLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( __state.Parameters, ref __result );
         }

         if( __result != null && __state.Parameters.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = __result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = __state.Parameters.Path;
         }
      }

#if MANAGED
      delegate AssetBundle OriginalMethod( byte[] binary, uint crc );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( byte[] binary, uint crc )
      {
         AssetBundle __result = null;
         AssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref binary, ref crc, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( binary, crc );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }













   internal static class AssetBundle_LoadFromStreamAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsyncInternal", typeof( Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsync", typeof( Stream ), typeof( uint ), typeof( uint ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsyncInternal", typeof( Il2CppSystem.IO.Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamAsync", typeof( Il2CppSystem.IO.Stream ), typeof( uint ), typeof( uint ) );
#endif
      }

#if MANAGED
      static bool Prefix( ref Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
#else
      static bool Prefix( ref Il2CppSystem.IO.Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
#endif
      {
         var parameters = new AssetBundleLoadingParameters( null, null, crc, 0, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         stream = p.Stream;
         crc = p.Crc;
         managedReadBufferSize = p.ManagedReadBufferSize;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleCreateRequest __result, ref AsyncAssetBundleLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetBundleLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleCreateRequest OriginalMethod( Stream stream, uint crc, uint managedReadBufferSize );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleCreateRequest MM_Detour( Stream stream, uint crc, uint managedReadBufferSize )
      {
         AssetBundleCreateRequest __result = null;
         AsyncAssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref stream, ref crc, ref managedReadBufferSize, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( stream, crc, managedReadBufferSize );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadFromStream_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamInternal", typeof( Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStream", typeof( Stream ), typeof( uint ), typeof( uint ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStreamInternal", typeof( Il2CppSystem.IO.Stream ), typeof( uint ), typeof( uint ) )
            ?? AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadFromStream", typeof( Il2CppSystem.IO.Stream ), typeof( uint ), typeof( uint ) );
#endif
      }

#if MANAGED
      static bool Prefix( ref Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundle __result, ref AssetBundleLoadingContext __state )
#else
      static bool Prefix( ref Il2CppSystem.IO.Stream stream, ref uint crc, ref uint managedReadBufferSize, ref AssetBundle __result, ref AssetBundleLoadingContext __state )
#endif
      {
         var parameters = new AssetBundleLoadingParameters( null, null, crc, 0, stream, managedReadBufferSize, AssetBundleLoadType.LoadFromMemory );
         __state = ResourceRedirection.Hook_AssetBundleLoading_Prefix( parameters, out __result );

         var p = __state.Parameters;
         stream = p.Stream;
         crc = p.Crc;
         managedReadBufferSize = p.ManagedReadBufferSize;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundle __result, ref AssetBundleLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetBundleLoaded_Postfix( __state.Parameters, ref __result );
         }

         if( __result != null && __state.Parameters.Path != null ) // should only be null if loaded through non-hooked methods
         {
            var ext = __result.GetOrCreateExtensionData<AssetBundleExtensionData>();
            ext.Path = __state.Parameters.Path;
         }
      }

#if MANAGED
      delegate AssetBundle OriginalMethod( Stream stream, uint crc, uint managedReadBufferSize );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundle MM_Detour( Stream stream, uint crc, uint managedReadBufferSize )
      {
         AssetBundle __result = null;
         AssetBundleLoadingContext __state = null;
         var callOriginal = Prefix( ref stream, ref crc, ref managedReadBufferSize, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( stream, crc, managedReadBufferSize );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }
















   internal static class AssetBundle_mainAsset_Hook
   {
      static bool Prepare( object instance )
      {
         var returnMainAsset = AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "returnMainAsset", new[] { typeof( AssetBundle ) } );
         return returnMainAsset == null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AssetBundle?.ClrType, "mainAsset" )?.GetGetMethod();
      }

      static bool Prefix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         var parameters = new AssetLoadingParameters( null, null, AssetLoadType.LoadMainAsset );
         __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );

         return !__state.SkipOriginalCall;
      }

      static void Postfix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( AssetBundle __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle __instance )
      {
         UnityEngine.Object __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_returnMainAsset_Hook
   {
      static bool Prepare( object instance )
      {
         var returnMainAsset = AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "returnMainAsset", new[] { typeof( AssetBundle ) } );
         return returnMainAsset != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "returnMainAsset", new[] { typeof( AssetBundle ) } );
      }

      static bool Prefix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         var parameters = new AssetLoadingParameters( null, null, AssetLoadType.LoadMainAsset );
         __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );

         return !__state.SkipOriginalCall;
      }

      static void Postfix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( AssetBundle __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle __instance )
      {
         UnityEngine.Object __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_Load_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "Load", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "Load", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
#endif
      {
         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle __instance, string name, Type type )
      {
         UnityEngine.Object __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAsync_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAsync", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAsync", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#endif
      {
         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         __state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, __instance, ref __result );

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleRequest OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle __instance, string name, Type type )
      {
         AssetBundleRequest __result = null;
         AsyncAssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAll_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAll", typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAll", typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref Type type, ref UnityEngine.Object[] __result, ref AssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref Il2CppSystem.Type type, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AssetLoadingContext __state )
#endif
      {
         var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
         __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );

         var p = __state.Parameters;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

#if MANAGED
      static void Postfix( AssetBundle __instance, ref UnityEngine.Object[] __result, ref AssetLoadingContext __state )
#else
      static void Postfix( AssetBundle __instance, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AssetLoadingContext __state )
#endif
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object[] OriginalMethod( AssetBundle __instance, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundle __instance, Type type )
      {
         UnityEngine.Object[] __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, type );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAsset_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAsset_Internal", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAsset_Internal", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
#endif
      {
         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( AssetBundle __instance, ref UnityEngine.Object __result, ref AssetLoadingContext __state )
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundle __instance, string name, Type type )
      {
         UnityEngine.Object __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAssetAsync_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetAsync_Internal", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetAsync_Internal", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#endif
      {
         var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamed );
         __state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, __instance, ref __result );

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleRequest OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle __instance, string name, Type type )
      {
         AssetBundleRequest __result = null;
         AsyncAssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAssetWithSubAssets_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetWithSubAssets_Internal", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetWithSubAssets_Internal", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref UnityEngine.Object[] __result, ref AssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AssetLoadingContext __state )
#endif
      {
         if( name == string.Empty )
         {
            var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
            __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );
         }
         else
         {
            var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamedWithSubAssets );
            __state = ResourceRedirection.Hook_AssetLoading_Prefix( parameters, __instance, ref __result );
         }

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

#if MANAGED
      static void Postfix( AssetBundle __instance, ref UnityEngine.Object[] __result, ref AssetLoadingContext __state )
#else
      static void Postfix( AssetBundle __instance, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AssetLoadingContext __state )
#endif
      {
         if( !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __instance, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object[] OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundle __instance, string name, Type type )
      {
         UnityEngine.Object[] __result = null;
         AssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( __instance, ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundle != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetWithSubAssetsAsync_Internal", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.AssetBundle?.ClrType, "LoadAssetWithSubAssetsAsync_Internal", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static bool Prefix( AssetBundle __instance, ref string name, ref Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#else
      static bool Prefix( AssetBundle __instance, ref string name, ref Il2CppSystem.Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
#endif
      {
         if( name == string.Empty )
         {
            var parameters = new AssetLoadingParameters( null, type, AssetLoadType.LoadByType );
            __state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, __instance, ref __result );
         }
         else
         {
            var parameters = new AssetLoadingParameters( name, type, AssetLoadType.LoadNamedWithSubAssets );
            __state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix( parameters, __instance, ref __result );
         }

         var p = __state.Parameters;
         name = p.Name;
         type = p.Type;

         return !__state.SkipOriginalCall;
      }

      static void Postfix( ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state )
      {
         ResourceRedirection.Hook_AssetLoading_Postfix( __state, __result );
      }

#if MANAGED
      delegate AssetBundleRequest OriginalMethod( AssetBundle __instance, string name, Type type );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static AssetBundleRequest MM_Detour( AssetBundle __instance, string name, Type type )
      {
         AssetBundleRequest __result = null;
         AsyncAssetLoadingContext __state = null;
         var callOriginal = Prefix( __instance, ref name, ref type, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance, name, type );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundleRequest_asset_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AssetBundleRequest != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AssetBundleRequest?.ClrType, "asset" )?.GetGetMethod();
      }

      static bool Prefix( AssetBundleRequest __instance, ref UnityEngine.Object __result, ref AsyncAssetLoadInfo __state )
      {
         if( ResourceRedirection.TryGetAssetBundleLoadInfo( __instance, out __state ) )
         {
            if( __state.ResolveType == AsyncAssetLoadingResolve.ThroughAssets )
            {
               var assets = __state.Assets;
               if( assets != null && assets.Length > 0 )
               {
                  __result = assets[ 0 ];
               }

               return false;
            }
            else
            {
               return true;
            }
         }
         else
         {
            return true;
         }
      }

      static void Postfix( ref UnityEngine.Object __result, ref AsyncAssetLoadInfo __state )
      {
         if( __state != null && !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __state.Bundle, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( AssetBundleRequest __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( AssetBundleRequest __instance )
      {
         UnityEngine.Object __result = null;
         AsyncAssetLoadInfo __state = null;
         var callOriginal = Prefix( __instance, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class AssetBundleRequest_allAssets_Hook
   {
      static bool Prepare( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AssetBundleRequest?.ClrType, "allAssets" ) != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AssetBundleRequest?.ClrType, "allAssets" )?.GetGetMethod();
      }

#if MANAGED
      static bool Prefix( AssetBundleRequest __instance, ref UnityEngine.Object[] __result, ref AsyncAssetLoadInfo __state )
#else
      static bool Prefix( AssetBundleRequest __instance, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AsyncAssetLoadInfo __state )
#endif
      {
         if( ResourceRedirection.TryGetAssetBundleLoadInfo( __instance, out __state ) )
         {
            if( __state.ResolveType == AsyncAssetLoadingResolve.ThroughAssets )
            {
               __result = __state.Assets;

               return false;
            }
            else
            {
               return true;
            }
         }
         else
         {
            return true;
         }
      }

#if MANAGED
      static void Postfix( ref UnityEngine.Object[] __result, ref AsyncAssetLoadInfo __state )
#else
      static void Postfix( ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result, ref AsyncAssetLoadInfo __state )
#endif
      {
         if( __state != null && !__state.SkipAllPostfixes )
         {
            ResourceRedirection.Hook_AssetLoaded_Postfix( __state.Parameters, __state.Bundle, ref __result );
         }
      }

#if MANAGED
      delegate UnityEngine.Object[] OriginalMethod( AssetBundleRequest __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( AssetBundleRequest __instance )
      {
         UnityEngine.Object[] __result = null;
         AsyncAssetLoadInfo __state = null;
         var callOriginal = Prefix( __instance, ref __result, ref __state );
         if( callOriginal )
         {
            __result = _original( __instance );
         }
         Postfix( ref __result, ref __state );

         return __result;
      }
#endif
   }

   internal static class Resources_Load_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Resources != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "Load", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "Load", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static void Postfix( string __0, Type __1, ref UnityEngine.Object __result )
#else
      static void Postfix( string __0, Il2CppSystem.Type __1, ref UnityEngine.Object __result )
#endif
      {
         var parameters = new ResourceLoadedParameters( __0, __1, ResourceLoadType.LoadNamed );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref __result );
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( string path, Type systemTypeInstance )
      {
         var __result = _original( path, systemTypeInstance );
         Postfix( path, systemTypeInstance, ref __result );

         return __result;
      }
#endif
   }

   internal static class Resources_LoadAll_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Resources != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "LoadAll", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "LoadAll", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static void Postfix( string __0, Type __1, ref UnityEngine.Object[] __result )
#else
      static void Postfix( string __0, Il2CppSystem.Type __1, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> __result )
#endif
      {
         var parameters = new ResourceLoadedParameters( __0, __1, ResourceLoadType.LoadByType );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref __result );
      }

#if MANAGED
      delegate UnityEngine.Object[] OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object[] MM_Detour( string path, Type systemTypeInstance )
      {
         var __result = _original( path, systemTypeInstance );
         Postfix( path, systemTypeInstance, ref __result );

         return __result;
      }
#endif
   }

   internal static class Resources_GetBuiltinResource_Old_Hook
   {
      static bool Prepare( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( Type ), typeof( string ) ) == null;
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( Il2CppSystem.Type ), typeof( string ) ) == null;
#endif
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( string ), typeof( Type ) );
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( string ), typeof( Il2CppSystem.Type ) );
#endif
      }

#if MANAGED
      static void Postfix( string __0, Type __1, ref UnityEngine.Object __result )
#else
      static void Postfix( string __0, Il2CppSystem.Type __1, ref UnityEngine.Object __result )
#endif
      {
         var parameters = new ResourceLoadedParameters( __0, __1, ResourceLoadType.LoadNamedBuiltIn );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref __result );
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( string path, Type systemTypeInstance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( string path, Type systemTypeInstance )
      {
         var __result = _original( path, systemTypeInstance );
         Postfix( path, systemTypeInstance, ref __result );

         return __result;
      }
#endif
   }

   internal static class Resources_GetBuiltinResource_New_Hook
   {
      static bool Prepare( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( string ), typeof( Type ) ) == null;
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( string ), typeof( Il2CppSystem.Type ) ) == null;
#endif
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( Type ), typeof( string ) );
#else
         return AccessToolsShim.Method( UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof( Il2CppSystem.Type ), typeof( string ) );
#endif
      }

#if MANAGED
      static void Postfix( Type __0, string __1, ref UnityEngine.Object __result )
#else
      static void Postfix( Il2CppSystem.Type __0, string __1, ref UnityEngine.Object __result )
#endif
      {
         var parameters = new ResourceLoadedParameters( __1, __0, ResourceLoadType.LoadNamedBuiltIn );
         ResourceRedirection.Hook_ResourceLoaded_Postfix( parameters, ref __result );
      }

#if MANAGED
      delegate UnityEngine.Object OriginalMethod( Type systemTypeInstance, string path );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static UnityEngine.Object MM_Detour( Type systemTypeInstance, string path )
      {
         var __result = _original( systemTypeInstance, path );
         Postfix( systemTypeInstance, path, ref __result );

         return __result;
      }
#endif
   }

   internal static class AsyncOperation_isDone_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "isDone" )?.GetGetMethod();
      }

      static bool Prefix( AsyncOperation __instance, ref bool __result )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance ) )
         {
            __result = true;

            return false;
         }

         return true;
      }

#if MANAGED
      delegate bool OriginalMethod( AsyncOperation __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( AsyncOperation __instance )
      {
         bool __result = default;
         if( Prefix( __instance, ref __result ) )
         {
            __result = _original( __instance );
         }
         return __result;
      }
#endif
   }

   internal static class AsyncOperation_progress_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "progress" )?.GetGetMethod();
      }

      static bool Prefix( AsyncOperation __instance, ref float __result )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance ) )
         {
            __result = 1.0f;

            return false;
         }

         return true;
      }

#if MANAGED
      delegate float OriginalMethod( AsyncOperation __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static float MM_Detour( AsyncOperation __instance )
      {
         float __result = default;
         if( Prefix( __instance, ref __result ) )
         {
            __result = _original( __instance );
         }

         return __result;
      }
#endif
   }

   internal static class AsyncOperation_priority_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "priority" )?.GetGetMethod();
      }

      static bool Prefix( AsyncOperation __instance, ref int __result )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance ) )
         {
            __result = 0;

            return false;
         }

         return true;
      }

#if MANAGED
      delegate int OriginalMethod( AsyncOperation __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static int MM_Detour( AsyncOperation __instance )
      {
         int __result = default;
         if( Prefix( __instance, ref __result ) )
         {
            __result = _original( __instance );
         }

         return __result;
      }
#endif
   }

   internal static class AsyncOperation_set_priority_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "priority" )?.GetSetMethod();
      }

      static bool Prefix( AsyncOperation __instance )
      {
         return !ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance );
      }

#if MANAGED
      delegate void OriginalMethod( AsyncOperation __instance, int value );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation __instance, int value )
      {
         if( Prefix( __instance ) )
         {
            _original( __instance, value );
         }
      }
#endif
   }

   internal static class AsyncOperation_allowSceneActivation_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "allowSceneActivation" )?.GetGetMethod();
      }

      static bool Prefix( AsyncOperation __instance, ref bool __result )
      {
         if( ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance ) )
         {
            __result = true;

            return false;
         }

         return true;
      }

#if MANAGED
      delegate bool OriginalMethod( AsyncOperation __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( AsyncOperation __instance )
      {
         bool __result = default;
         if( Prefix( __instance, ref __result ) )
         {
            __result = _original( __instance );
         }
         return __result; // do not believe this has an impact
      }
#endif
   }

   internal static class AsyncOperation_set_allowSceneActivation_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.AsyncOperation?.ClrType, "allowSceneActivation" )?.GetSetMethod();
      }

      static bool Prefix( AsyncOperation __instance )
      {
         return !ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance );
      }

#if MANAGED
      delegate void OriginalMethod( AsyncOperation __instance, bool value );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation __instance, bool value )
      {
         if( Prefix( __instance ) )
         {
            _original( __instance, value );
         }
      }
#endif
   }

   internal static class AsyncOperation_Finalize_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.AsyncOperation != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return UnityTypes.AsyncOperation?.ClrType.GetMethod( "Finalize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
      }

      static bool Prefix( AsyncOperation __instance )
      {
         return !ResourceRedirection.ShouldBlockAsyncOperationMethods( __instance );
      }

#if MANAGED
      delegate void OriginalMethod( AsyncOperation __instance );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( AsyncOperation __instance )
      {
         if( Prefix( __instance ) )
         {
            _original( __instance );
         }
      }
#endif
   }
}
