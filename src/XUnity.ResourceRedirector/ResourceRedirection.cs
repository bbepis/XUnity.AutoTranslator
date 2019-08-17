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
   public class ResourceRedirection
   {
      private static readonly object Sync = new object();
      private static readonly WeakDictionary<AssetBundleRequest, AsyncAssetBundleLoadInfo> AssetBundleRequestToAssetBundle = new WeakDictionary<AssetBundleRequest, AsyncAssetBundleLoadInfo>();

      private protected static readonly List<Action<AssetLoadedContext>> RedirectionsForAssets = new List<Action<AssetLoadedContext>>();
      private protected static readonly List<Action<ResourceLoadedContext>> RedirectionsForResources = new List<Action<ResourceLoadedContext>>();
      private protected static readonly List<Action<AssetBundleLoadingContext>> RedirectionsForAssetBundles = new List<Action<AssetBundleLoadingContext>>();
      private protected static readonly List<Action<AsyncAssetBundleLoadingContext>> RedirectionsForAsyncAssetBundles = new List<Action<AsyncAssetBundleLoadingContext>>();
      private static bool _initialized = false;
      private static bool _logUnhandledResources = false;

      private static bool _isFiringAssetLoadedEvent = false;
      private static bool _isFiringResourceLoadedEvent = false;
      private static bool _isFiringAssetBundleLoadingEvent = false;
      private static bool _isFiringAsyncAssetBundleLoadingEvent = false;

      /// <summary>
      /// Gets or sets a bool indicating if the resource redirector
      /// should log which assets has no handler.
      /// </summary>
      public static bool LogUnhandledResources
      {
         get
         {
            return _logUnhandledResources;
         }
         set
         {
            if( value )
            {
               Initialize();
            }

            _logUnhandledResources = value;
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

      internal static void Hook_AssetBundleLoaded_Postfix( string path, uint crc, ulong offset, AssetBundleLoadType loadType, out AssetBundle bundle )
      {
         if( !_isFiringAssetBundleLoadingEvent )
         {
            _isFiringAssetBundleLoadingEvent = true;
            try
            {
               var context = new AssetBundleLoadingContext( path, crc, offset, loadType );

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

               if( context.Handled )
               {
                  bundle = context.Bundle;
                  return;
               }
            }
            finally
            {
               _isFiringAssetBundleLoadingEvent = false;
            }
         }

         if( _logUnhandledResources )
         {
            XuaLogger.ResourceRedirector.Debug( $"Found asset bundle that no AssetBundleLoading handler could handle ({path})." );
         }


         bundle = null;
      }

      internal static void Hook_AssetBundleLoading_Postfix( string path, uint crc, ulong offset, AssetBundleLoadType loadType, out AssetBundleCreateRequest request )
      {
         if( !_isFiringAsyncAssetBundleLoadingEvent )
         {
            _isFiringAsyncAssetBundleLoadingEvent = true;
            try
            {
               var context = new AsyncAssetBundleLoadingContext( path, crc, offset, loadType );

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
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AsyncAssetBundleLoading event." );
                  }
               }

               if( context.Handled )
               {
                  request = context.Request;
                  return;
               }
            }
            finally
            {
               _isFiringAsyncAssetBundleLoadingEvent = false;
            }
         }

         if( _logUnhandledResources )
         {
            XuaLogger.ResourceRedirector.Debug( $"Found asset bundle that no AsyncAssetBundleLoading handler could handle ({path})." );
         }

         request = null;
      }

      internal static void Hook_AssetLoaded_Postfix( string assetName, Type assetType, AssetLoadType loadType, AssetBundle parentBundle, AssetBundleRequest request, ref UnityEngine.Object asset )
      {
         if( asset == null ) return;

         var arr = new[] { asset };

         Hook_AssetLoaded_Postfix( assetName, assetType, loadType, parentBundle, request, ref arr );

         asset = arr[ 0 ];
      }

      internal static void Hook_AssetLoaded_Postfix( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle, AssetBundleRequest request, ref UnityEngine.Object[] assets )
      {
         if( assets == null ) return;

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

         if( loadType == AssetLoadType.LoadByType )
         {
            for( int i = 0; i < assets.Length; i++ )
            {
               var asset = assets[ i ];
               var arr = new[] { asset };
               FireAssetLoadedEvent( assetName, assetType, bundle, loadType, ref arr );
               assets[ i ] = arr[ 0 ];
            }
         }
         else
         {
            FireAssetLoadedEvent( assetName, assetType, bundle, loadType, ref assets );
         }
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
         if( asset == null ) return;

         var arr = new[] { asset };

         Hook_ResourceLoaded_Postfix( assetPath, assetType, loadType, ref arr );

         asset = arr[ 0 ];
      }

      internal static void Hook_ResourceLoaded_Postfix( string assetPath, Type assetType, ResourceLoadType loadType, ref UnityEngine.Object[] assets )
      {
         if( assets == null ) return;

         if( loadType == ResourceLoadType.LoadByType )
         {
            for( int i = 0; i < assets.Length; i++ )
            {
               var asset = assets[ i ];
               var arr = new[] { asset };
               FireResourceLoadedEvent( assetPath, assetType, loadType, ref arr );
               assets[ i ] = arr[ 0 ];
            }
         }
         else
         {
            FireResourceLoadedEvent( assetPath, assetType, loadType, ref assets );
         }
      }

      internal static void FireAssetLoadedEvent( string assetLoadName, Type assetLoadType, AssetBundle assetBundle, AssetLoadType loadType, ref UnityEngine.Object[] assets )
      {
         if( !_isFiringAssetLoadedEvent )
         {
            _isFiringAssetLoadedEvent = true;
            var context = new AssetLoadedContext( assetLoadName, assetLoadType, loadType, assetBundle, assets );

            try
            {
               var list2 = RedirectionsForAssets;
               var len2 = list2.Count;
               for( int i = 0; i < len2; i++ )
               {
                  try
                  {
                     var redirection = list2[ i ];

                     redirection( context );

                     if( context.Handled ) return;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
                  }
               }

               if( !context.Handled && _logUnhandledResources )
               {
                  foreach( var asset in assets )
                  {
                     var uniquePath = context.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Found asset that no AssetLoaded handler could handle '{asset.GetType().FullName}' loaded through '{loadType.ToString()}' ({uniquePath})." );
                  }
               }

               if( context.Handled )
               {
                  assets = context.Assets;
               }
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AssetLoaded event." );
            }
            finally
            {
               assets = context.Assets; // Only GETTER ... for now...

               foreach( var asset in assets )
               {
                  var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                  ext.HasBeenRedirected = true;
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
            var context = new ResourceLoadedContext( resourceLoadPath, resourceLoadType, loadType, assets );

            try
            {
               var list2 = RedirectionsForResources;
               var len2 = list2.Count;
               for( int i = 0; i < len2; i++ )
               {
                  try
                  {
                     var redirection = list2[ i ];

                     redirection( context );

                     if( context.Handled ) return;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking ResourceLoaded event." );
                  }
               }

               if( !context.Handled && _logUnhandledResources )
               {
                  foreach( var asset in assets )
                  {
                     var uniquePath = context.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Found asset that no ResourceLoaded handler could handle '{asset.GetType().FullName}' loaded through '{loadType.ToString()}' ({uniquePath})." );
                  }
               }

               if( context.Handled )
               {
                  assets = context.Assets;
               }
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking ResourceLoaded event." );
            }
            finally
            {
               assets = context.Assets; // Only GETTER ... for now...

               foreach( var asset in assets )
               {
                  var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                  ext.HasBeenRedirected = true;
               }

               _isFiringResourceLoadedEvent = false;
            }
         }
      }

      /// <summary>
      /// Register ReourceLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterResourceLoadedHook( Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForResources.Add( action );
      }

      /// <summary>
      /// Unregister ReourceLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterResourceLoadedHook( Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForResources.Remove( action );
      }

      /// <summary>
      /// Register AssetLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadedHook( Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAssets.Add( action );
      }

      /// <summary>
      /// Unregister AssetLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadedHook( Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAssets.Remove( action );
      }

      /// <summary>
      /// Register AssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAseetBundleLoadingHook( Action<AssetBundleLoadingContext> action )
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
      public static void RegisterAsyncAseetBundleLoadingHook( Action<AsyncAssetBundleLoadingContext> action )
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
