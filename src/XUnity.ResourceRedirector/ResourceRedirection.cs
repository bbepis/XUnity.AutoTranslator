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
      private static readonly string LoweredCurrentDirectory = Environment.CurrentDirectory.ToLowerInvariant();
      private static readonly object Sync = new object();
      private static readonly MethodInfo CallHandleAssetOrResource_Method = typeof( ResourceRedirection ).GetMethod( "CallHandleAssetOrResource", BindingFlags.Static | BindingFlags.NonPublic );
      private static readonly WeakDictionary<AssetBundleRequest, AssetBundle> AssetBundleRequestToAssetBundle = new WeakDictionary<AssetBundleRequest, AssetBundle>();

      private protected static readonly List<Action<AssetLoadedContext>> RedirectionsForAllAssets = new List<Action<AssetLoadedContext>>();
      private protected static readonly List<Action<AssetBundleLoadingContext>> RedirectionsForAssetBundles = new List<Action<AssetBundleLoadingContext>>();
      private protected static readonly List<Action<AsyncAssetBundleLoadingContext>> RedirectionsForAsyncAssetBundles = new List<Action<AsyncAssetBundleLoadingContext>>();
      private static bool _initialized = false;
      private static bool _logUnhandledResources = false;

      private static bool _isFiringAssetLoadedEvent = false;
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

      internal static void Hook_AssetBundleLoaded_Postfix( string path, out AssetBundle bundle )
      {
         if( !_isFiringAssetBundleLoadingEvent )
         {
            _isFiringAssetBundleLoadingEvent = true;
            try
            {
               var normalizedPath = path.ToLowerInvariant().MakeRelativePath( LoweredCurrentDirectory );
               var context = new AssetBundleLoadingContext( path, normalizedPath );

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

      internal static void Hook_AssetBundleLoading_Postfix( string path, out AssetBundleCreateRequest request )
      {
         if( !_isFiringAsyncAssetBundleLoadingEvent )
         {
            _isFiringAsyncAssetBundleLoadingEvent = true;
            try
            {
               var normalizedPath = path.ToLowerInvariant().MakeRelativePath( LoweredCurrentDirectory );
               var context = new AsyncAssetBundleLoadingContext( path, normalizedPath );

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

      internal static void Hook_AssetLoaded_Postfix( AssetBundle parentBundle, AssetBundleRequest request, ref UnityEngine.Object asset )
      {
         if( asset == null ) return;

         lock( Sync )
         {
            if( parentBundle == null )
            {
               if( !AssetBundleRequestToAssetBundle.TryGetValue( request, out parentBundle ) )
               {
                  XuaLogger.ResourceRedirector.Error( "Could not find asset bundle from request object!" );
                  return;
               }
            }
         }

         var path = parentBundle.name;

         // Is this a problem???
         if( path == null )
         {
            XuaLogger.ResourceRedirector.Error( "Could not obtain path for asset bundle!" );
            return;
         }

         FireAssetOrResourceLoadedEvent( path, parentBundle, AssetSource.AssetBundle, ref asset );
      }

      internal static void Hook_AssetLoading( AssetBundle parentBundle, AssetBundleRequest request )
      {
         // create ref from request to parentBundle?
         lock( Sync )
         {
            if( parentBundle != null && request != null )
            {
               AssetBundleRequestToAssetBundle[ request ] = parentBundle;
            }
         }
      }

      internal static void Hook_ResourceLoaded( string path, bool pathIncludesAssetName, ref UnityEngine.Object asset )
      {
         if( asset == null ) return;

         if( pathIncludesAssetName )
         {
            // remove asset name
            path = path.ToLowerInvariant();
            var loweredAssetName = asset.name.ToLowerInvariant();

            if( path.EndsWith( loweredAssetName ) )
            {
               path = path.Remove( path.Length - loweredAssetName.Length ).TrimEnd( '\\', '/' );
            }
         }

         FireAssetOrResourceLoadedEvent( path, null, AssetSource.Resources, ref asset );
      }

      internal static void FireAssetOrResourceLoadedEvent( string path, AssetBundle assetBundle, AssetSource source, ref UnityEngine.Object asset )
      {
         // prevent recursion from plugins

         if( !_isFiringAssetLoadedEvent )
         {
            _isFiringAssetLoadedEvent = true;
            try
            {
               var type = asset.GetType();

               // make relative and normalize
               path = path.ToLowerInvariant();

               var args = new object[] { asset, path, assetBundle, source };
               CallHandleAssetOrResource_Method
                  .MakeGenericMethod( type )
                  .Invoke( null, args );

               asset = (UnityEngine.Object)args[ 0 ];
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while redirecting resource." );
            }
            finally
            {
               _isFiringAssetLoadedEvent = false;
            }
         }
      }


      internal static void CallHandleAssetOrResource<TAsset>( ref TAsset asset, string path, AssetBundle assetBundle, AssetSource source )
         where TAsset : UnityEngine.Object
      {
         var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
         try
         {
            var context = new AssetLoadedContext<TAsset>( source, path, assetBundle, asset, ext.HasBeenRedirected );

            ResourceRedirection<TAsset>.Invoke( context );

            if( !context.Handled && _logUnhandledResources )
            {
               XuaLogger.ResourceRedirector.Debug( $"Found asset that no AssetLoaded handler could handle '{typeof( TAsset ).FullName}' ({context.UniqueFileSystemAssetPath})." );
            }

            if( context.Handled )
            {
               asset = context.Asset;
            }
         }
         finally
         {
            ext.HasBeenRedirected = true;
         }
      }

      /// <summary>
      /// Register AssetLoaded event for asset of specific type.
      /// </summary>
      /// <typeparam name="TAsset">The type of asset to listen to.</typeparam>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadedFor<TAsset>( Action<AssetLoadedContext<TAsset>> action )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirection<TAsset>.AddRedirection( action );
      }

      /// <summary>
      /// Unregister AssetLoaded event for asset of specific type.
      /// </summary>
      /// <typeparam name="TAsset">The type of asset to stop listening to.</typeparam>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadedFor<TAsset>( Action<AssetLoadedContext<TAsset>> action )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirection<TAsset>.RemoveRedirection( action );
      }

      /// <summary>
      /// Register AssetLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAseetLoadedForAll( Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAllAssets.Add( action );
      }

      /// <summary>
      /// Unregister AssetLoaded event for all assets.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadedForAll( Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAllAssets.Remove( action );
      }

      /// <summary>
      /// Register AssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAseetBundleLoadingForAll( Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAssetBundles.Add( action );
      }

      /// <summary>
      /// Unregister AssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetBundleLoadingForAll( Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAssetBundles.Remove( action );
      }

      /// <summary>
      /// Register AsyncAssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAseetBundleLoadingForAll( Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAsyncAssetBundles.Add( action );
      }

      /// <summary>
      /// Unregister AsyncAssetBundleLoading event.
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAssetBundleLoadingForAll( Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForAsyncAssetBundles.Remove( action );
      }
   }

   internal class ResourceRedirection<TAsset> : ResourceRedirection
      where TAsset : UnityEngine.Object
   {
      private static readonly List<Action<AssetLoadedContext<TAsset>>> RedirectionsForSpecificType = new List<Action<AssetLoadedContext<TAsset>>>();

      /// <summary>
      /// Adds a resource redirection handler.
      /// </summary>
      /// <param name="action">The action to be invoked.</param>
      public static void AddRedirection( Action<AssetLoadedContext<TAsset>> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForSpecificType.Add( action );
      }

      /// <summary>
      /// Removes a previously added redirection handler.
      /// </summary>
      /// <param name="action">The action that was previously added.</param>
      public static void RemoveRedirection( Action<AssetLoadedContext<TAsset>> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         RedirectionsForSpecificType.Remove( action );
      }

      internal static void Invoke( AssetLoadedContext<TAsset> context )
      {
         var list1 = RedirectionsForSpecificType;
         var len1 = list1.Count;
         for( int i = 0; i < len1; i++ )
         {
            try
            {
               var redirection = list1[ i ];

               redirection( context );

               if( context.Handled ) return;
            }
            catch( Exception ex )
            {
               XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
            }
         }

         var list2 = RedirectionsForAllAssets;
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
      }
   }
}
