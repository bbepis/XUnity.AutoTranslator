using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   public class ResourceRedirector
   {
      private static readonly string CurrentDirectoryLowered = Environment.CurrentDirectory.ToLowerInvariant();
      private static readonly object Sync = new object();
      private static readonly MethodInfo CallHandleAssetOrResource_Method = typeof( ResourceRedirector ).GetMethod( "CallHandleAssetOrResource", BindingFlags.Static | BindingFlags.NonPublic );
      private static readonly WeakDictionary<AssetBundleCreateRequest, string> AssetBundleCreateRequestToPath = new WeakDictionary<AssetBundleCreateRequest, string>();
      private static readonly WeakDictionary<AssetBundle, string> AssetBundleToPath = new WeakDictionary<AssetBundle, string>();
      private static readonly WeakDictionary<AssetBundleRequest, string> AssetBundleRequestToPath = new WeakDictionary<AssetBundleRequest, string>();
      private static bool _initialized = false;

      internal static void Initialize()
      {
         if( !_initialized )
         {
            _initialized = true;

            HooksSetup.InstallResourceAndAssetHooks();
            StartMaintenance();
         }
      }

      private static void StartMaintenance()
      {
         // start a thread that will periodically removed unused references
         var t1 = new Thread( MaintenanceLoop );
         t1.IsBackground = true;
         t1.Start();
      }

      private static void MaintenanceLoop( object state )
      {
         while( true )
         {
            try
            {
               Cull();
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An unexpected error occurred while removing GC'ed resources." );
            }

            Thread.Sleep( 1000 * 60 * 3 );
         }
      }

      internal static void Cull()
      {
         lock( Sync )
         {
            AssetBundleCreateRequestToPath.RemoveCollectedEntries();
            AssetBundleRequestToPath.RemoveCollectedEntries();
            AssetBundleToPath.RemoveCollectedEntries();
         }
      }

      internal static void Hook_AssetBundleLoaded( string path, AssetBundle bundle, AssetBundleCreateRequest request )
      {
         // path may be null! Should be obtained from request object in that case
         if( path == null )
         {
            lock( Sync )
            {
               AssetBundleCreateRequestToPath.TryGetValue( request, out path );
            }
         }

         if( bundle != null && path != null )
         {
            lock( Sync )
            {
               AssetBundleToPath[ bundle ] = path;
            }
         }
      }

      internal static void Hook_AssetBundleLoading( string path, AssetBundleCreateRequest request )
      {
         // create assocation from request to path
         if( request != null && path != null )
         {
            lock( Sync )
            {
               AssetBundleCreateRequestToPath[ request ] = path;
            }
         }
      }

      internal static void Hook_AssetLoaded( AssetBundle parentBundle, AssetBundleRequest request, ref UnityEngine.Object asset )
      {
         if( asset == null ) return;

         // parentBundle may be null, obtain from request
         string path = null;
         if( parentBundle != null )
         {
            lock( Sync )
            {
               AssetBundleToPath.TryGetValue( parentBundle, out path );
            }
         }
         else if( request != null )
         {
            lock( Sync )
            {
               AssetBundleRequestToPath.TryGetValue( request, out path );
            }
         }

         if( path == null )
         {
            XuaLogger.Current.Error( "Could not obtain path for asset bundle!" );
            return;
         }

         string fullPath;
         if( string.IsNullOrEmpty( asset.name ) )
         {
            fullPath = Path.Combine( "Assets", path );
         }
         else
         {
            fullPath = Path.Combine( Path.Combine( "Assets", path ), asset.name );
         }


         HandleAssetOrResource( fullPath, ref asset );
      }

      internal static void Hook_AssetLoading( AssetBundle parentBundle, AssetBundleRequest request )
      {
         // create ref from request to parentBundle?
         lock( Sync )
         {
            if( parentBundle != null && AssetBundleToPath.TryGetValue( parentBundle, out var path ) && request != null && path != null )
            {
               AssetBundleRequestToPath[ request ] = path;
            }
         }
      }

      internal static void Hook_ResourceLoaded( string path, ref UnityEngine.Object asset )
      {
         if( asset == null ) return;

         var fullPath = Path.Combine( "Resources", path );

         HandleAssetOrResource( fullPath, ref asset );
      }

      internal static void HandleAssetOrResource( string path, ref UnityEngine.Object asset )
      {
         try
         {
            var type = asset.GetType();

            var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
            if( !ext.HasBeenRedirected )
            {
               try
               {
                  // make relative and normalize
                  path = path.ToLowerInvariant().MakeRelativePath( CurrentDirectoryLowered );

                  var args = new object[] { path, asset };
                  CallHandleAssetOrResource_Method
                     .MakeGenericMethod( type )
                     .Invoke( null, args );

                  asset = (UnityEngine.Object)args[ 1 ];
               }
               catch( Exception e )
               {
                  XuaLogger.Current.Error( e, "An error occurred while redirecting resource." );
               }
               finally
               {
                  ext.HasBeenRedirected = true;
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while handling potentially redirected resource." );
         }
      }


      internal static void CallHandleAssetOrResource<TAsset>( string path, ref TAsset asset )
         where TAsset : UnityEngine.Object
      {
         var context = new RedirectionContext<TAsset>( path, asset );

         ResourceRedirector<TAsset>.Invoke( context );

         if( !context.Handled && Settings.EnableDumping )
         {
            XuaLogger.Current.Info( $"Found resource that no resource redirection handler can handle '{typeof( TAsset ).FullName}' ({path})." );
         }

         asset = context.Asset;
      }


      public static void AddRedirection<TAsset>( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirector<TAsset>.AddRedirection( action, ignoredHandled );
      }

      public static void RemoveRedirection<TAsset>( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirector<TAsset>.RemoveRedirection( action, ignoredHandled );
      }
   }

   /// <summary>
   /// The ResourceRedirector class allows you to subscribe to resource redirection events
   /// for specific types of resources.
   /// </summary>
   /// <typeparam name="TAsset"></typeparam>
   public class ResourceRedirector<TAsset> : ResourceRedirector
      where TAsset : UnityEngine.Object
   {
      private static List<ResourceRedirectionCallback> _redirections = new List<ResourceRedirectionCallback>();

      /// <summary>
      /// Event that is invoked when an asset is loading.
      /// </summary>
      public static event Action<IRedirectionContext<TAsset>> AssetLoading
      {
         add
         {
            AddRedirection( value, false );
         }
         remove
         {
            RemoveRedirection( value, false );
         }
      }

      /// <summary>
      /// Adds a resource redirection handler.
      /// </summary>
      /// <param name="action">The action to be invoked.</param>
      /// <param name="ignoredHandled">A bool indicating whether or not the action should be invoked even if the resource redirection has already been handled.</param>
      public static void AddRedirection( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         _redirections.Add( new ResourceRedirectionCallback( action, ignoredHandled ) );
      }

      /// <summary>
      /// Removes a previously added redirection handler.
      /// </summary>
      /// <param name="action">The action that was previously added.</param>
      /// <param name="ignoredHandled">The same value the redirection handler was previously added with.</param>
      public static void RemoveRedirection( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var redirection = new ResourceRedirectionCallback( action, ignoredHandled );
         _redirections.Remove( redirection );
      }

      internal static void Invoke( IRedirectionContext<TAsset> context )
      {
         var list = _redirections;
         var len = list.Count;
         for( int i = 0; i < len; i++ )
         {
            try
            {
               var redirection = list[ i ];

               if( !context.Handled || redirection.IgnoreHandled )
               {
                  redirection.Action( context );
               }
            }
            catch( Exception ex )
            {
               XuaLogger.Current.Error( ex, "An error occurred while invoking OnAssetLoading event." );
            }
         }
      }

      private class ResourceRedirectionCallback : IEquatable<ResourceRedirectionCallback>
      {
         public ResourceRedirectionCallback( Action<IRedirectionContext<TAsset>> action, bool ignoreHandled )
         {
            Action = action;
            IgnoreHandled = ignoreHandled;
         }

         public Action<IRedirectionContext<TAsset>> Action { get; }

         public bool IgnoreHandled { get; set; }

         public override bool Equals( object obj )
         {
            return Equals( obj as ResourceRedirectionCallback );
         }

         public bool Equals( ResourceRedirectionCallback other )
         {
            return other != null &&
                    EqualityComparer<Action<IRedirectionContext<TAsset>>>.Default.Equals( Action, other.Action ) &&
                     IgnoreHandled == other.IgnoreHandled;
         }

         public override int GetHashCode()
         {
            var hashCode = 1426693738;
            hashCode = hashCode * -1521134295 + EqualityComparer<Action<IRedirectionContext<TAsset>>>.Default.GetHashCode( Action );
            hashCode = hashCode * -1521134295 + IgnoreHandled.GetHashCode();
            return hashCode;
         }
      }
   }
}
