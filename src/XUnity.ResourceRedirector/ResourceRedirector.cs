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
   public class ResourceRedirectionManager
   {
      private static readonly string CurrentDirectoryLowered = Environment.CurrentDirectory.ToLowerInvariant();
      private static readonly object Sync = new object();
      private static readonly MethodInfo CallHandleAssetOrResource_Method = typeof( ResourceRedirectionManager ).GetMethod( "CallHandleAssetOrResource", BindingFlags.Static | BindingFlags.NonPublic );
      private static readonly WeakDictionary<AssetBundleCreateRequest, string> AssetBundleCreateRequestToPath = new WeakDictionary<AssetBundleCreateRequest, string>();
      private static readonly WeakDictionary<AssetBundle, string> AssetBundleToPath = new WeakDictionary<AssetBundle, string>();
      private static readonly WeakDictionary<AssetBundleRequest, string> AssetBundleRequestToPath = new WeakDictionary<AssetBundleRequest, string>();

      private protected static readonly List<ResourceRedirectionCallback> RedirectionsForAll = new List<ResourceRedirectionCallback>();
      private static bool _initialized = false;
      private static bool _logUnhandledResources = false;

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
            AssetBundleCreateRequestToPath.RemoveCollectedEntries();
            AssetBundleRequestToPath.RemoveCollectedEntries();
            AssetBundleToPath.RemoveCollectedEntries();
         }
      }

      internal static void Hook_AssetBundleLoaded_Postfix( string path, AssetBundle bundle, AssetBundleCreateRequest request )
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

      internal static void Hook_AssetBundleLoading_Postfix( string path, AssetBundleCreateRequest request )
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

      internal static void Hook_AssetLoaded_Postfix( AssetBundle parentBundle, AssetBundleRequest request, ref UnityEngine.Object asset )
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
            XuaLogger.ResourceRedirector.Error( "Could not obtain path for asset bundle!" );
            return;
         }

         if( !string.IsNullOrEmpty( asset.name ) )
         {
            path = Path.Combine( path, asset.name );
         }
         else
         {
            XuaLogger.ResourceRedirector.Error( "ASSET HAS A NAME:" );
            XuaLogger.ResourceRedirector.Error( "Path: " + path );
            XuaLogger.ResourceRedirector.Error( "Name: " + asset.name );
         }

         HandleAssetOrResource( path, AssetSource.AssetBundle, ref asset );
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

         HandleAssetOrResource( path, AssetSource.Resources, ref asset );
      }

      internal static void HandleAssetOrResource( string path, AssetSource source, ref UnityEngine.Object asset )
      {
         try
         {
            var type = asset.GetType();

            try
            {
               // make relative and normalize
               path = path.ToLowerInvariant().MakeRelativePath( CurrentDirectoryLowered );

               var args = new object[] { asset, path, source };
               CallHandleAssetOrResource_Method
                  .MakeGenericMethod( type )
                  .Invoke( null, args );

               asset = (UnityEngine.Object)args[ 0 ];
            }
            catch( Exception e )
            {
               XuaLogger.ResourceRedirector.Error( e, "An error occurred while redirecting resource." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while handling potentially redirected resource." );
         }
      }


      internal static void CallHandleAssetOrResource<TAsset>( ref TAsset asset, string path, AssetSource source )
         where TAsset : UnityEngine.Object
      {
         var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
         try
         {
            var context = new RedirectionContext<TAsset>( source, path, asset, ext.HasBeenRedirected );

            ResourceRedirectionManager<TAsset>.Invoke( context );

            if( !context.Handled && _logUnhandledResources )
            {
               XuaLogger.ResourceRedirector.Debug( $"Found resource that no resource redirection handler can handle '{typeof( TAsset ).FullName}' ({path})." );
            }

            asset = context.Asset;
         }
         finally
         {
            ext.HasBeenRedirected = true;
         }
      }


      public static void AddRedirectionFor<TAsset>( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirectionManager<TAsset>.AddRedirection( action, ignoredHandled );
      }

      public static void RemoveRedirectionFor<TAsset>( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
         where TAsset : UnityEngine.Object
      {
         ResourceRedirectionManager<TAsset>.RemoveRedirection( action, ignoredHandled );
      }

      public static void AddRedirectionForAll( Action<IRedirectionContext> action, bool ignoredHandled )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         Initialize();

         RedirectionsForAll.Add( new ResourceRedirectionCallback( action, ignoredHandled ) );
      }

      public static void RemoveRedirectionForAll( Action<IRedirectionContext> action, bool ignoredHandled )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var redirection = new ResourceRedirectionCallback( action, ignoredHandled );
         RedirectionsForAll.Remove( redirection );
      }

      private protected class ResourceRedirectionCallback : IEquatable<ResourceRedirectionCallback>
      {
         public ResourceRedirectionCallback( Action<IRedirectionContext> action, bool ignoreHandled )
         {
            Action = action;
            IgnoreHandled = ignoreHandled;
         }

         public Action<IRedirectionContext> Action { get; }

         public bool IgnoreHandled { get; set; }

         public override bool Equals( object obj )
         {
            return Equals( obj as ResourceRedirectionCallback );
         }

         public bool Equals( ResourceRedirectionCallback other )
         {
            return other != null &&
                    EqualityComparer<Action<IRedirectionContext>>.Default.Equals( Action, other.Action ) &&
                     IgnoreHandled == other.IgnoreHandled;
         }

         public override int GetHashCode()
         {
            var hashCode = 1426693738;
            hashCode = hashCode * -1521134295 + EqualityComparer<Action<IRedirectionContext>>.Default.GetHashCode( Action );
            hashCode = hashCode * -1521134295 + IgnoreHandled.GetHashCode();
            return hashCode;
         }
      }
   }

   /// <summary>
   /// The ResourceRedirector class allows you to subscribe to resource redirection events
   /// for specific types of resources.
   /// </summary>
   /// <typeparam name="TAsset"></typeparam>
   public class ResourceRedirectionManager<TAsset> : ResourceRedirectionManager
      where TAsset : UnityEngine.Object
   {
      private static readonly List<GenericResourceRedirectionCallback> RedirectionsForSpecificType = new List<GenericResourceRedirectionCallback>();

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

         RedirectionsForSpecificType.Add( new GenericResourceRedirectionCallback( action, ignoredHandled ) );
      }

      /// <summary>
      /// Removes a previously added redirection handler.
      /// </summary>
      /// <param name="action">The action that was previously added.</param>
      /// <param name="ignoredHandled">The same value the redirection handler was previously added with.</param>
      public static void RemoveRedirection( Action<IRedirectionContext<TAsset>> action, bool ignoredHandled )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var redirection = new GenericResourceRedirectionCallback( action, ignoredHandled );
         RedirectionsForSpecificType.Remove( redirection );
      }

      internal static void Invoke( IRedirectionContext<TAsset> context )
      {
         var list1 = RedirectionsForSpecificType;
         var len1 = list1.Count;
         for( int i = 0; i < len1; i++ )
         {
            try
            {
               var redirection = list1[ i ];

               if( !context.Handled || redirection.IgnoreHandled )
               {
                  redirection.Action( context );
               }
            }
            catch( Exception ex )
            {
               XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking OnAssetLoading event." );
            }
         }

         var list2 = RedirectionsForAll;
         var len2 = list2.Count;
         for( int i = 0; i < len2; i++ )
         {
            try
            {
               var redirection = list2[ i ];

               if( !context.Handled || redirection.IgnoreHandled )
               {
                  redirection.Action( context );
               }
            }
            catch( Exception ex )
            {
               XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking OnAssetLoading event." );
            }
         }
      }

      private class GenericResourceRedirectionCallback : IEquatable<GenericResourceRedirectionCallback>
      {
         public GenericResourceRedirectionCallback( Action<IRedirectionContext<TAsset>> action, bool ignoreHandled )
         {
            Action = action;
            IgnoreHandled = ignoreHandled;
         }

         public Action<IRedirectionContext<TAsset>> Action { get; }

         public bool IgnoreHandled { get; set; }

         public override bool Equals( object obj )
         {
            return Equals( obj as GenericResourceRedirectionCallback );
         }

         public bool Equals( GenericResourceRedirectionCallback other )
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
