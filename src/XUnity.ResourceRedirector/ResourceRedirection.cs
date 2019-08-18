using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector.Hooks;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Entrypoint to the resource redirection API.
   /// </summary>
   public static class ResourceRedirection
   {
      private static readonly object Sync = new object();
      private static readonly WeakDictionary<AssetBundleRequest, AsyncAssetBundleLoadInfo> AssetBundleRequestToAssetBundle = new WeakDictionary<AssetBundleRequest, AsyncAssetBundleLoadInfo>();
      private static readonly List<Action<AssetLoadedContext>> RedirectionsForAssetsPerCall = new List<Action<AssetLoadedContext>>();
      private static readonly List<Action<AssetLoadedContext>> RedirectionsForAssetsPerResource = new List<Action<AssetLoadedContext>>();
      private static readonly List<Action<ResourceLoadedContext>> RedirectionsForResourcesPerCall = new List<Action<ResourceLoadedContext>>();
      private static readonly List<Action<ResourceLoadedContext>> RedirectionsForResourcesPerResource = new List<Action<ResourceLoadedContext>>();
      private static readonly List<Action<AssetBundleLoadingContext>> RedirectionsForAssetBundles = new List<Action<AssetBundleLoadingContext>>();
      private static readonly List<Action<AsyncAssetBundleLoadingContext>> RedirectionsForAsyncAssetBundles = new List<Action<AsyncAssetBundleLoadingContext>>();
      private static bool _initialized = false;
      private static bool _logAllLoadedResources = false;

      private static bool _isFiringAssetLoadedEvent = false;
      private static bool _isFiringResourceLoadedEvent = false;
      private static bool _isFiringAssetBundleLoadingEvent = false;
      private static bool _isFiringAsyncAssetBundleLoadingEvent = false;

      /// <summary>
      /// Gets or sets a bool indicating if the resource redirector
      /// should log all loaded resources/assets to the console.
      /// </summary>
      public static bool LogAllLoadedResources
      {
         get
         {
            return _logAllLoadedResources;
         }
         set
         {
            if( value )
            {
               Initialize();
            }

            _logAllLoadedResources = value;
         }
      }

      internal static void Initialize()
      {
         if( !_initialized )
         {
            _initialized = true;

            HookingHelper.PatchAll( ResourceAndAssetHooks.All, false );

            MaintenanceHelper.AddMaintenanceFunction( Cull, 12 );
         }
      }

      internal static void Cull()
      {
         lock( Sync )
         {
            AssetBundleRequestToAssetBundle.RemoveCollectedEntries();
         }
      }

      internal static bool Hook_AssetBundleLoaded_Prefix( string path, uint crc, ulong offset, AssetBundleLoadType loadType, out AssetBundle bundle )
      {
         if( !_isFiringAssetBundleLoadingEvent )
         {
            _isFiringAssetBundleLoadingEvent = true;

            try
            {
               var context = new AssetBundleLoadingContext( path, crc, offset, loadType );
               if( _logAllLoadedResources )
               {
                  XuaLogger.ResourceRedirector.Debug( $"Loading Asset Bundle: ({context.GetNormalizedPath()})." );
               }

               var list2 = RedirectionsForAssetBundles;
               var len2 = list2.Count;
               for( int i = 0; i < len2; i++ )
               {
                  try
                  {
                     var redirection = list2[ i ];

                     redirection( context );

                     if( context.Handled ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetBundleLoading event." );
                  }
               }

               bundle = context.Bundle;

               if( context.Handled ) return true;
            }
            finally
            {
               _isFiringAssetBundleLoadingEvent = false;
            }
         }

         bundle = null;
         return false;
      }

      internal static bool Hook_AssetBundleLoading_Prefix( string path, uint crc, ulong offset, AssetBundleLoadType loadType, out AssetBundleCreateRequest request )
      {
         if( !_isFiringAsyncAssetBundleLoadingEvent )
         {
            _isFiringAsyncAssetBundleLoadingEvent = true;

            try
            {
               var context = new AsyncAssetBundleLoadingContext( path, crc, offset, loadType );
               if( _logAllLoadedResources )
               {
                  XuaLogger.ResourceRedirector.Debug( $"Loading Asset Bundle (async): ({context.GetNormalizedPath()})." );
               }

               var list2 = RedirectionsForAsyncAssetBundles;
               var len2 = list2.Count;
               for( int i = 0; i < len2; i++ )
               {
                  try
                  {
                     var redirection = list2[ i ];

                     redirection( context );

                     if( context.Handled ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetBundleLoading event." );
                  }
               }

               request = context.Request;

               if( context.Handled ) return true;
            }
            finally
            {
               _isFiringAsyncAssetBundleLoadingEvent = false;
            }
         }

         request = null;
         return false;
      }

      internal static void Hook_AssetLoaded_Postfix( string assetName, Type assetType, AssetLoadType loadType, AssetBundle parentBundle, AssetBundleRequest request, ref UnityEngine.Object asset )
      {
         UnityEngine.Object[] arr;
         if( asset == null )
         {
            arr = new UnityEngine.Object[ 0 ];
         }
         else
         {
            arr = new[] { asset };
         }

         Hook_AssetLoaded_Postfix( assetName, assetType, loadType, parentBundle, request, ref arr );

         if( arr == null || arr.Length == 0 )
         {
            asset = null;
         }
         else if( arr.Length > 1 )
         {
            XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in AssetLoaded event. Returned more than one asset to call requiring only a single asset." );
            asset = arr[ 0 ];
         }
         else if( arr.Length == 1 )
         {
            asset = arr[ 0 ];
         }
      }

      internal static void Hook_AssetLoaded_Postfix( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle, AssetBundleRequest request, ref UnityEngine.Object[] assets )
      {
         lock( Sync )
         {
            if( bundle == null )
            {
               if( !AssetBundleRequestToAssetBundle.TryGetValue( request, out var loadInfo ) )
               {
                  XuaLogger.ResourceRedirector.Error( "Could not find asset bundle from request object!" );
                  return;
               }
               else
               {
                  bundle = loadInfo.Bundle;
                  assetName = loadInfo.AssetName;
                  assetType = loadInfo.AssetType;
                  loadType = loadInfo.LoadType;
               }
            }
         }

         FireAssetLoadedEvent( assetName, assetType, bundle, loadType, ref assets );
      }

      internal static void Hook_AssetLoading( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle, AssetBundleRequest request )
      {
         // create ref from request to parentBundle?
         lock( Sync )
         {
            if( bundle != null && request != null )
            {
               AssetBundleRequestToAssetBundle[ request ] = new AsyncAssetBundleLoadInfo( assetName, assetType, loadType, bundle );
            }
         }
      }

      internal static void Hook_ResourceLoaded_Postfix( string assetPath, Type assetType, ResourceLoadType loadType, ref UnityEngine.Object asset )
      {
         UnityEngine.Object[] arr;
         if( asset == null )
         {
            arr = new UnityEngine.Object[ 0 ];
         }
         else
         {
            arr = new[] { asset };
         }

         Hook_ResourceLoaded_Postfix( assetPath, assetType, loadType, ref arr );

         if( arr == null || arr.Length == 0 )
         {
            asset = null;
         }
         else if( arr.Length > 1 )
         {
            XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in ResourceLoaded event. Returned more than one asset to call requiring only a single asset." );
            asset = arr[ 0 ];
         }
         else if( arr.Length == 1 )
         {
            asset = arr[ 0 ];
         }
      }

      internal static void Hook_ResourceLoaded_Postfix( string assetPath, Type assetType, ResourceLoadType loadType, ref UnityEngine.Object[] assets )
      {
         FireResourceLoadedEvent( assetPath, assetType, loadType, ref assets );
      }

      internal static void FireAssetLoadedEvent( string assetLoadName, Type assetLoadType, AssetBundle assetBundle, AssetLoadType loadType, ref UnityEngine.Object[] assets )
      {
         if( !_isFiringAssetLoadedEvent )
         {
            _isFiringAssetLoadedEvent = true;

            var originalAssets = assets?.ToArray();
            try
            {
               var contextPerCall = new AssetLoadedContext( assetLoadName, assetLoadType, loadType, assetBundle, assets );

               if( _logAllLoadedResources && assets != null )
               {
                  for( int i = 0; i < assets.Length; i++ )
                  {
                     var asset = assets[ i ];
                     var uniquePath = contextPerCall.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Loaded Asset: '{asset.GetType().FullName}', Load Type: '{loadType.ToString()}', Unique Path: ({uniquePath})." );
                  }
               }

               // handle "per call" hooks first
               var list1 = RedirectionsForAssetsPerCall;
               var len1 = list1.Count;
               for( int i = 0; i < len1; i++ )
               {
                  try
                  {
                     var redirection = list1[ i ];

                     redirection( contextPerCall );

                     if( contextPerCall.Assets.Contains( null ) )
                     {
                        XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in AssetLoaded event. If you want to remove an asset from the array, replace the entire array." );
                     }

                     if( contextPerCall.Handled ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
                  }
               }

               assets = contextPerCall.Assets;

               // handle "per resource" hooks afterwards
               if( !contextPerCall.Handled && assets != null )
               {
                  for( int j = 0; j < assets.Length; j++ )
                  {
                     var asset = new[] { assets[ j ] };
                     if( asset != null )
                     {
                        var contextPerResource = new AssetLoadedContext( assetLoadName, assetLoadType, loadType, assetBundle, asset );

                        var list2 = RedirectionsForAssetsPerResource;
                        var len2 = list2.Count;
                        for( int i = 0; i < len2; i++ )
                        {
                           try
                           {
                              var redirection = list2[ i ];

                              redirection( contextPerResource );

                              if( contextPerResource.Assets != null && contextPerResource.Assets.Length == 1 && contextPerResource.Assets[ 0 ] != null )
                              {
                                 assets[ j ] = contextPerResource.Assets[ 0 ];
                              }
                              else
                              {
                                 XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in AssetLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}." );
                              }

                              if( contextPerResource.Handled ) break;
                           }
                           catch( Exception ex )
                           {
                              XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
                           }
                        }
                     }
                     else
                     {
                        XuaLogger.ResourceRedirector.Error( "Found unexpected null asset during AssetLoaded event." );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AssetLoaded event." );
            }
            finally
            {
               if( originalAssets != null )
               {
                  foreach( var asset in originalAssets )
                  {
                     var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                     ext.HasBeenRedirected = true;
                  }
               }

               _isFiringAssetLoadedEvent = false;
            }
         }
      }

      internal static void FireResourceLoadedEvent( string resourceLoadPath, Type resourceLoadType, ResourceLoadType loadType, ref UnityEngine.Object[] assets )
      {
         if( !_isFiringResourceLoadedEvent )
         {
            _isFiringResourceLoadedEvent = true;

            var originalAssets = assets?.ToArray();
            try
            {
               var contextPerCall = new ResourceLoadedContext( resourceLoadPath, resourceLoadType, loadType, assets );

               if( _logAllLoadedResources && assets != null )
               {
                  for( int i = 0; i < assets.Length; i++ )
                  {
                     var asset = assets[ i ];
                     var uniquePath = contextPerCall.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Loaded Asset: '{asset.GetType().FullName}', Load Type: '{loadType.ToString()}', Unique Path: ({uniquePath})." );
                  }
               }

               // handle "per call" hooks first
               var list1 = RedirectionsForResourcesPerCall;
               var len1 = list1.Count;
               for( int i = 0; i < len1; i++ )
               {
                  try
                  {
                     var redirection = list1[ i ];

                     redirection( contextPerCall );

                     if( contextPerCall.Assets.Contains( null ) )
                     {
                        XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in ResourceLoaded event. If you want to remove an asset from the array, replace the entire array." );
                     }

                     if( contextPerCall.Handled ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking ResourceLoaded event." );
                  }
               }

               assets = contextPerCall.Assets;

               // handle "per resource" hooks afterwards
               if( !contextPerCall.Handled && assets != null )
               {
                  for( int j = 0; j < assets.Length; j++ )
                  {
                     var asset = new[] { assets[ j ] };
                     if( asset != null )
                     {
                        var contextPerResource = new ResourceLoadedContext( resourceLoadPath, resourceLoadType, loadType, asset );

                        var list2 = RedirectionsForResourcesPerResource;
                        var len2 = list2.Count;
                        for( int i = 0; i < len2; i++ )
                        {
                           try
                           {
                              var redirection = list2[ i ];

                              redirection( contextPerResource );

                              if( contextPerResource.Assets != null && contextPerResource.Assets.Length == 1 && contextPerResource.Assets[ 0 ] != null )
                              {
                                 assets[ j ] = contextPerResource.Assets[ 0 ];
                              }
                              else
                              {
                                 XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in ResourceLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}." );
                              }

                              if( contextPerResource.Handled ) break;
                           }
                           catch( Exception ex )
                           {
                              XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking ResourceLoaded event." );
                           }
                        }
                     }
                     else
                     {
                        XuaLogger.ResourceRedirector.Error( "Found unexpected null asset during ResourceLoaded event." );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking ResourceLoaded event." );
            }
            finally
            {
               if( originalAssets != null )
               {
                  foreach( var asset in originalAssets )
                  {
                     var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                     ext.HasBeenRedirected = true;
                  }
               }

               _isFiringResourceLoadedEvent = false;
            }
         }
      }

      /// <summary>
      /// Register ReourceLoaded event.
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterResourceLoadedHook( HookBehaviour behaviour, Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            RedirectionsForResourcesPerCall.Add( action );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            RedirectionsForResourcesPerResource.Add( action );
         }
      }

      /// <summary>
      /// Unregister ReourceLoaded event.
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void UnregisterResourceLoadedHook( HookBehaviour behaviour, Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            RedirectionsForResourcesPerCall.Remove( action );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            RedirectionsForResourcesPerResource.Remove( action );
         }
      }

      /// <summary>
      /// Register AssetLoaded event.
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadedHook( HookBehaviour behaviour, Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            RedirectionsForAssetsPerCall.Add( action );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            RedirectionsForAssetsPerResource.Add( action );
         }
      }

      /// <summary>
      /// Unregister AssetLoaded event.
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadedHook( HookBehaviour behaviour, Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            RedirectionsForAssetsPerCall.Remove( action );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            RedirectionsForAssetsPerResource.Remove( action );
         }
      }

      /// <summary>
      /// Register AssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetBundleLoadingHook( Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAssetBundles.Add( action );
      }

      /// <summary>
      /// Unregister AssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetBundleLoadingHook( Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAssetBundles.Remove( action );
      }

      /// <summary>
      /// Register AsyncAssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAssetBundleLoadingHook( Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAsyncAssetBundles.Add( action );
      }

      /// <summary>
      /// Unregister AsyncAssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAssetBundleLoadingHook( Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAsyncAssetBundles.Remove( action );
      }
   }
}
