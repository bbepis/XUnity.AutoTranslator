using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
      private static readonly List<PrioritizedCallback<Action<AssetLoadedContext>>> PostfixRedirectionsForAssetsPerCall = new List<PrioritizedCallback<Action<AssetLoadedContext>>>();
      private static readonly List<PrioritizedCallback<Action<AssetLoadedContext>>> PostfixRedirectionsForAssetsPerResource = new List<PrioritizedCallback<Action<AssetLoadedContext>>>();
      private static readonly List<PrioritizedCallback<Action<ResourceLoadedContext>>> PostfixRedirectionsForResourcesPerCall = new List<PrioritizedCallback<Action<ResourceLoadedContext>>>();
      private static readonly List<PrioritizedCallback<Action<ResourceLoadedContext>>> PostfixRedirectionsForResourcesPerResource = new List<PrioritizedCallback<Action<ResourceLoadedContext>>>();

      private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAssetsPerCall = new List<PrioritizedCallback<Delegate>>();
      private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAsyncAssetsPerCall = new List<PrioritizedCallback<Delegate>>();

      private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAssetBundles = new List<PrioritizedCallback<Delegate>>();
      private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAsyncAssetBundles = new List<PrioritizedCallback<Delegate>>();

      private static readonly List<PrioritizedCallback<Action<AssetBundleLoadedContext>>> PostfixRedirectionsForAssetBundles = new List<PrioritizedCallback<Action<AssetBundleLoadedContext>>>();

      private static Action<AssetBundleLoadingContext> _emulateAssetBundles;
      private static Action<AsyncAssetBundleLoadingContext> _emulateAssetBundlesAsync;
      private static Action<AssetBundleLoadingContext> _redirectionMissingAssetBundlesToEmpty;
      private static Action<AsyncAssetBundleLoadingContext> _redirectionMissingAssetBundlesToEmptyAsync;

      private static bool _enabledRandomizeCabIfConflict = false;
      private static Action<AssetBundleLoadingContext> _enableCabRandomizationPrefix;
      private static Action<AsyncAssetBundleLoadingContext> _enableCabRandomizationPrefixAsync;
      private static Action<AssetBundleLoadedContext> _enableCabRandomizationPostfix;

      private static bool _initialized = false;
      private static bool _initializedSyncOverAsyncEnabled = false;
      private static bool _logAllLoadedResources = false;
      private static bool _isFiringAssetBundle;
      private static bool _isFiringResource;
      private static bool _isFiringAsset;
      private static bool _isRecursionDisabledPermanently;

      internal static bool RecursionEnabled = true;
      internal static bool SyncOverAsyncEnabled = false;

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

      /// <summary>
      /// Gets or sets a bool indicating if the log callback
      /// order should be logged everytime a new callback is added.
      /// </summary>
      public static bool LogCallbackOrder { get; set; }

      /// <summary>
      /// Initializes the Resource Redirector.
      /// </summary>
      public static void Initialize()
      {
         if( !_initialized )
         {
            _initialized = true;

            HookingHelper.PatchAll( ResourceAndAssetHooks.GeneralHooks, false );
         }
      }

      /// <summary>
      /// Enables experimental hooks that allows returning an Asset instead of a Request from async prefix
      /// asset load operations.
      /// </summary>
      public static void EnableSyncOverAsyncAssetLoads()
      {
         Initialize();

         if( !_initializedSyncOverAsyncEnabled )
         {
            _initializedSyncOverAsyncEnabled = true;
            SyncOverAsyncEnabled = true;

            HookingHelper.PatchAll( ResourceAndAssetHooks.SyncOverAsyncHooks, false );
         }
      }

      /// <summary>
      /// Disables all recursive behaviour in the plugin. This means that trying to load an asset
      /// using the hooked APIs will not trigger a callback back into the callback chain. This makes
      /// setting the correct priorities on callbacks much more important.
      ///
      /// This method should not be called lightly. It should not be something a single plugin randomly
      /// decides to call, but rather decision for how to use the ResourceRedirection API on a game-wide basis.
      /// </summary>
      public static void DisableRecursionPermanently()
      {
         _isRecursionDisabledPermanently = true;
      }

      //public static void EnableHighPoly1()
      //{
      //   ResourceRedirection.RegisterAssetLoadingHook( 0, ctx => HandleAssetRedirection( ctx, SetAsset ) );
      //   ResourceRedirection.RegisterAsyncAssetLoadingHook( 0, ctx => HandleAssetRedirection( ctx, SetRequest ) );

      //   void HandleAssetRedirection<TContext>( TContext context, Func<TContext, string, bool> changeAsset )
      //      where TContext : IAssetLoadingContext
      //   {
      //      var param = context.Parameters;
      //      var name = param.Name;
      //      if( param.LoadType == AssetLoadType.LoadNamed && name.EndsWith( "_low" ) )
      //      {
      //         var newName = name.Substring( 0, name.Length - 4 );
      //         var ok = changeAsset( context, newName );
      //         if( ok )
      //         {
      //            context.Complete(
      //               skipRemainingPrefixes: true,
      //               skipOriginalCall: true,
      //               skipAllPostfixes: true );
      //         }
      //      }
      //   }

      //   // synchronous specific code
      //   bool SetRequest( AsyncAssetLoadingContext context, string newName )
      //   {
      //      var request = context.Bundle.LoadAssetAsync( newName );
      //      if( request != null )
      //      {
      //         context.Request = request;
      //         return true;
      //      }
      //      return false;
      //   }

      //   // asynchronous specific code
      //   bool SetAsset( AssetLoadingContext context, string newName )
      //   {
      //      var asset = context.Bundle.LoadAsset( newName );
      //      if( asset != null )
      //      {
      //         context.Asset = asset;
      //         return true;
      //      }
      //      return false;
      //   }
      //}

      //public static void EnableHighPoly2()
      //{
      //   ResourceRedirection.RegisterAssetLoadingHook( -5, HandleAssetRedirection );
      //   ResourceRedirection.RegisterAsyncAssetLoadingHook( -5, HandleAssetRedirection );

      //   void HandleAssetRedirection<TContext>( TContext context )
      //      where TContext : IAssetLoadingContext
      //   {
      //      var param = context.Parameters;
      //      var name = param.Name;
      //      if( param.LoadType == AssetLoadType.LoadNamed && name.EndsWith( "_low" ) )
      //      {
      //         param.Name = name.Substring( 0, name.Length - 4 );

      //         // we do not call complete here at all, as we will allow any other
      //         // handler to handle this request after us
      //      }
      //   }
      //}

      //public static void EnableAsyncToSyncRedirector( int priority )
      //{
      //   ResourceRedirection.EnableSyncOverAsyncAssetLoads();

      //   RegisterAsyncAndSyncAssetLoadingHook( context =>
      //   {
      //      XuaLogger.ResourceRedirector.Warn( "Redirecting asset: " + context.Parameters.LoadType );

      //      if( context.Parameters.LoadType == AssetLoadType.LoadNamed )
      //      {
      //         context.Asset = context.Bundle.LoadAsset( context.Parameters.Name, context.Parameters.Type );
      //      }
      //      else if( context.Parameters.LoadType == AssetLoadType.LoadByType )
      //      {
      //         context.Assets = context.Bundle.LoadAllAssets( context.Parameters.Type );
      //      }
      //      else if( context.Parameters.LoadType == AssetLoadType.LoadNamedWithSubAssets )
      //      {
      //         context.Assets = context.Bundle.LoadAssetWithSubAssets( context.Parameters.Name, context.Parameters.Type );
      //      }
      //      else
      //      {
      //         context.Asset = context.Bundle.mainAsset;
      //      }

      //      context.Complete();
      //   } );
      //}

      //public static void EnableEmulateAssetBundles2( int priority, string emulationDirectory )
      //{
      //   RegisterAsyncAndSyncAssetBundleLoadingHook( priority, context =>
      //   {
      //      if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile )
      //      {
      //         var normalizedPath = context.GetNormalizedPath();
      //         var emulatedPath = Path.Combine( emulationDirectory, normalizedPath );
      //         if( File.Exists( emulatedPath ) )
      //         {
      //            XuaLogger.ResourceRedirector.Warn( context.GetType().Name );

      //            context.Bundle = AssetBundle.LoadFromFile( emulatedPath, context.Parameters.Crc, context.Parameters.Offset );

      //            context.Complete(
      //               skipRemainingPrefixes: true,
      //               skipOriginalCall: true );

      //            XuaLogger.ResourceRedirector.Debug( "Redirected asset bundle: '" + context.Parameters.Path + "' => '" + emulatedPath + "'" );
      //         }
      //      }
      //   } );
      //}

      //public static void Whatever()
      //{
      //   ResourceRedirection.EnableSyncOverAsyncAssetLoads();

      //   ResourceRedirection.RegisterAsyncAndSyncAssetLoadingHook( context =>
      //   {
      //      XuaLogger.ResourceRedirector.Warn( context.Bundle.name );
      //      XuaLogger.ResourceRedirector.Warn( context.GetNormalizedAssetBundlePath() );
      //      XuaLogger.ResourceRedirector.Warn( context.GetAssetBundlePath() );
      //      XuaLogger.ResourceRedirector.Warn( "--------------------" );

      //   } );
      //}

      /// <summary>
      /// Creates an asset bundle hook that attempts to load asset bundles in the emulation directory
      /// over the default asset bundles if they exist.
      /// </summary>
      /// <param name="priority">Priority of the hook.</param>
      /// <param name="emulationDirectory">The directory to look for the asset bundles in.</param>
      public static void EnableEmulateAssetBundles( int priority, string emulationDirectory )
      {
         if( _emulateAssetBundles == null && _emulateAssetBundlesAsync == null )
         {
            _emulateAssetBundles = ctx => HandleAssetBundleEmulation( ctx, SetBundle );
            _emulateAssetBundlesAsync = ctx => HandleAssetBundleEmulation( ctx, SetRequest );

            RegisterAssetBundleLoadingHook( priority, _emulateAssetBundles );
            RegisterAsyncAssetBundleLoadingHook( priority, _emulateAssetBundlesAsync );

            // define base callback
            void HandleAssetBundleEmulation<T>( T context, Action<T, string> changeBundle )
               where T : IAssetBundleLoadingContext
            {
               if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile )
               {
                  var normalizedPath = context.GetNormalizedPath();
                  var emulatedPath = Path.Combine( emulationDirectory, normalizedPath );
                  if( File.Exists( emulatedPath ) )
                  {
                     changeBundle( context, emulatedPath );

                     context.Complete(
                        skipRemainingPrefixes: true,
                        skipOriginalCall: true );

                     XuaLogger.ResourceRedirector.Debug( "Redirected asset bundle: '" + context.Parameters.Path + "' => '" + emulatedPath + "'" );
                  }
               }
            }

            // synchronous specific code
            void SetBundle( AssetBundleLoadingContext context, string path )
            {
               context.Bundle = AssetBundle.LoadFromFile( path, context.Parameters.Crc, context.Parameters.Offset );
            }

            // asynchronous specific code
            void SetRequest( AsyncAssetBundleLoadingContext context, string path )
            {
               context.Request = AssetBundle.LoadFromFileAsync( path, context.Parameters.Crc, context.Parameters.Offset );
            }
         }
      }

      /// <summary>
      /// Disable a previously enabled asset bundle emulation.
      /// </summary>
      public static void DisableEmulateAssetBundles()
      {
         if( _emulateAssetBundles != null && _emulateAssetBundlesAsync != null )
         {
            UnregisterAssetBundleLoadingHook( _emulateAssetBundles );
            UnregisterAsyncAssetBundleLoadingHook( _emulateAssetBundlesAsync );

            _emulateAssetBundles = null;
            _emulateAssetBundlesAsync = null;
         }
      }

      /// <summary>
      /// Creates an asset bundle hook that redirects asset bundles loads to an empty
      /// asset bundle if the file that is being loaded does not exist.
      /// </summary>
      /// <param name="priority">Priority of the hook.</param>
      public static void EnableRedirectMissingAssetBundlesToEmptyAssetBundle( int priority )
      {
         if( _redirectionMissingAssetBundlesToEmpty == null && _redirectionMissingAssetBundlesToEmptyAsync == null )
         {
            _redirectionMissingAssetBundlesToEmpty = ctx => HandleMissingBundle( ctx, SetBundle );
            _redirectionMissingAssetBundlesToEmptyAsync = ctx => HandleMissingBundle( ctx, SetRequest );

            RegisterAssetBundleLoadingHook( priority, _redirectionMissingAssetBundlesToEmpty );
            RegisterAsyncAssetBundleLoadingHook( priority, _redirectionMissingAssetBundlesToEmptyAsync );

            // define base callback
            void HandleMissingBundle<TContext>( TContext context, Action<TContext, byte[]> changeBundle )
               where TContext : IAssetBundleLoadingContext
            {
               if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile
                  && !File.Exists( context.Parameters.Path ) )
               {
                  var buffer = Properties.Resources.empty;
                  CabHelper.RandomizeCab( buffer );

                  changeBundle( context, buffer );

                  context.Complete(
                     skipRemainingPrefixes: true,
                     skipOriginalCall: true );

                  XuaLogger.ResourceRedirector.Warn( "Tried to load non-existing asset bundle: " + context.Parameters.Path );
               }
            }

            // synchronous specific code
            void SetBundle( AssetBundleLoadingContext context, byte[] assetBundleData )
            {
               var bundle = AssetBundle.LoadFromMemory( assetBundleData );
               context.Bundle = bundle;
            }

            // asynchronous specific code
            void SetRequest( AsyncAssetBundleLoadingContext context, byte[] assetBundleData )
            {
               var request = AssetBundle.LoadFromMemoryAsync( assetBundleData );
               context.Request = request;
            }
         }
      }

      /// <summary>
      /// Enables CAB randomization of loaded asset bundles if a conflict is detected.
      /// </summary>
      /// <param name="priority">Priority of the hook.</param>
      /// <param name="forceRandomizeWhenInMemory">Indicates whether to force all asset bundles already in memory to have their CAB randomized regardless of whether there is a conflict.</param>
      public static void EnableRandomizeCabIfConflict( int priority, bool forceRandomizeWhenInMemory )
      {
         if( !_enabledRandomizeCabIfConflict )
         {
            _enabledRandomizeCabIfConflict = true;

            if( forceRandomizeWhenInMemory )
            {
               _enableCabRandomizationPrefix = ctx => HandleCabRandomizePrefix( ctx );
               _enableCabRandomizationPrefixAsync = ctx => HandleCabRandomizePrefix( ctx );

               RegisterAssetBundleLoadingHook( priority, _enableCabRandomizationPrefix );
               RegisterAsyncAssetBundleLoadingHook( priority, _enableCabRandomizationPrefixAsync );
            }

            _enableCabRandomizationPostfix = ctx => HandleCabRandomizePostfix( ctx );

            RegisterAssetBundleLoadedHook( priority, _enableCabRandomizationPostfix );

            // define base callback
            void HandleCabRandomizePrefix( IAssetBundleLoadingContext context )
            {
               if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromMemory )
               {
                  CabHelper.RandomizeCabWithAnyLength( context.Parameters.Binary );

                  // NOTE: Since we let the original call handle the invokation, we do not complete at all
               }
            }

            void HandleCabRandomizePostfix( AssetBundleLoadedContext context )
            {
               if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile )
               {
                  // we did not load any bundle, but the file exists, lets try from in-memory!
                  if( context.Bundle == null && File.Exists( context.Parameters.Path ) )
                  {
                     XuaLogger.ResourceRedirector.Warn( $"The asset bundle '{context.Parameters.Path}' could not be loaded likely due to conflicting CAB-string. Retrying in-memory with randomized CAB-string." );

                     byte[] buffer;
                     using( var stream = new FileStream( context.Parameters.Path, FileMode.Open, FileAccess.Read ) )
                     {
                        var fullLength = stream.Length;
                        var offset = (long)context.Parameters.Offset;
                        var lengthToRead = fullLength - offset;
                        stream.Seek( offset, SeekOrigin.Begin );
                        buffer = stream.ReadFully( (int)lengthToRead );
                     }

                     // only need to randomize in case we are not forcing the randomization due to recursion
                     if( !forceRandomizeWhenInMemory )
                     {
                        CabHelper.RandomizeCabWithAnyLength( buffer );
                     }

                     var bundle = AssetBundle.LoadFromMemory( buffer, 0 ); // dont pass crc, since we modified the bundle!
                     if( bundle != null )
                     {
                        context.Bundle = bundle;
                        context.Complete(
                           skipRemainingPostfixes: true );
                     }
                  }
               }
               else if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromMemory )
               {
                  if( context.Bundle == null && !forceRandomizeWhenInMemory )
                  {
                     var name = AssetBundleHelper.PathForLoadedInMemoryBundle ?? "Unnamed";

                     XuaLogger.ResourceRedirector.Warn( $"Could not load an in-memory asset bundle ({name}) likely due to conflicting CAB-string. Retrying with randomized CAB-string." );

                     CabHelper.RandomizeCabWithAnyLength( context.Parameters.Binary );

                     var bundle = AssetBundle.LoadFromMemory( context.Parameters.Binary, 0 ); // dont pass crc, since we modified the bundle!
                     if( bundle != null )
                     {
                        context.Bundle = bundle;
                        context.Complete(
                           skipRemainingPostfixes: true );
                     }
                  }
               }
               else if( context.Parameters.LoadType == AssetBundleLoadType.LoadFromStream )
               {
                  if( context.Bundle == null )
                  {
                     var name = AssetBundleHelper.PathForLoadedInMemoryBundle ?? "Unnamed";

                     XuaLogger.ResourceRedirector.Warn( $"Could not load a stream asset bundle ({name}) likely due to conflicting CAB-string. Retrying with randomized CAB-string." );

                     var binary = context.Parameters.Stream.ReadFully( 0 );
                     if( !forceRandomizeWhenInMemory )
                     {
                        // if randomization is foced, this is handled through other callback
                        CabHelper.RandomizeCabWithAnyLength( binary );
                     }

                     var bundle = AssetBundle.LoadFromMemory( binary, 0 ); // dont pass crc, since we modified the bundle!
                     if( bundle != null )
                     {
                        context.Bundle = bundle;
                        context.Complete(
                           skipRemainingPostfixes: true );
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Disables CAB randomization if it was previously enabled.
      /// </summary>
      public static void DisableRandomizeCabIfConflict()
      {
         if( _enabledRandomizeCabIfConflict )
         {
            _enabledRandomizeCabIfConflict = false;

            if( _enableCabRandomizationPrefix != null )
            {
               UnregisterAssetBundleLoadingHook( _enableCabRandomizationPrefix );
               _enableCabRandomizationPrefix = null;
            }

            if( _enableCabRandomizationPrefixAsync != null )
            {
               UnregisterAsyncAssetBundleLoadingHook( _enableCabRandomizationPrefixAsync );
               _enableCabRandomizationPrefixAsync = null;
            }

            if( _enableCabRandomizationPostfix != null )
            {
               UnregisterAssetBundleLoadedHook( _enableCabRandomizationPostfix );
               _enableCabRandomizationPostfix = null;
            }
         }
      }

      /// <summary>
      /// Disable a previously enabled redirect missing asset bundles to empty asset bundle.
      /// </summary>
      public static void DisableRedirectMissingAssetBundlesToEmptyAssetBundle()
      {
         if( _redirectionMissingAssetBundlesToEmpty != null && _redirectionMissingAssetBundlesToEmptyAsync != null )
         {
            UnregisterAssetBundleLoadingHook( _redirectionMissingAssetBundlesToEmpty );
            UnregisterAsyncAssetBundleLoadingHook( _redirectionMissingAssetBundlesToEmptyAsync );

            _redirectionMissingAssetBundlesToEmpty = null;
            _redirectionMissingAssetBundlesToEmptyAsync = null;
         }
      }

      internal static bool TryGetAssetBundleLoadInfo( AssetBundleRequest request, out AsyncAssetLoadInfo result )
      {
         result = request.GetExtensionData<AsyncAssetLoadInfo>();
         return result != null;
      }

      internal static bool TryGetAssetBundle( AssetBundleCreateRequest request, out AsyncAssetBundleLoadInfo result )
      {
         result = request.GetExtensionData<AsyncAssetBundleLoadInfo>();
         return result != null;
      }

      internal static bool ShouldBlockAsyncOperationMethods( AssetBundleRequest operation )
      {
         return TryGetAssetBundleLoadInfo( operation, out var result ) && result.ResolveType == AsyncAssetLoadingResolve.ThroughAssets;
      }

      internal static bool ShouldBlockAsyncOperationMethods( AssetBundleCreateRequest operation )
      {
         return TryGetAssetBundle( operation, out var result ) && result.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle;
      }

      internal static bool ShouldBlockAsyncOperationMethods( AsyncOperation operation )
      {
         return SyncOverAsyncEnabled
            && (
               ( operation.TryCastTo<AssetBundleRequest>( out var r1 ) && ShouldBlockAsyncOperationMethods( r1 ) )
               ||
               ( operation.TryCastTo<AssetBundleCreateRequest>( out var r2 ) && ShouldBlockAsyncOperationMethods( r2 ) )
            );
      }

      internal static AssetBundleLoadingContext Hook_AssetBundleLoading_Prefix( AssetBundleLoadingParameters parameters, out AssetBundle bundle )
      {
         var context = new AssetBundleLoadingContext( parameters );

         if( _isFiringAssetBundle && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
         {
            bundle = null;
            return context;
         }

         try
         {
            _isFiringAssetBundle = true;

            if( _logAllLoadedResources )
            {
               XuaLogger.ResourceRedirector.Debug( $"Loading Asset Bundle: ({context.GetNormalizedPath()})." );
            }

            var list2 = PrefixRedirectionsForAssetBundles;
            var len2 = list2.Count;
            for( int i = 0; i < len2; i++ )
            {
               var redirection = list2[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     if( redirection.Callback is Action<AssetBundleLoadingContext> c1 )
                     {
                        c1( context );
                     }
                     else if( redirection.Callback is Action<IAssetBundleLoadingContext> c2 )
                     {
                        c2( context );
                     }

                     if( context.SkipRemainingPrefixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetBundleLoading event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AssetBundleLoading event." );
         }
         finally
         {
            _isFiringAssetBundle = false;
         }

         bundle = context.Bundle;
         return context;
      }

      internal static AssetBundleLoadedContext Hook_AssetBundleLoaded_Postfix( AssetBundleLoadingParameters parameters, ref AssetBundle bundle )
      {
         var context = new AssetBundleLoadedContext( parameters, bundle );

         if( _isFiringAssetBundle && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
         {
            bundle = null;
            return context;
         }

         try
         {
            _isFiringAssetBundle = true;

            var list2 = PostfixRedirectionsForAssetBundles;
            var len2 = list2.Count;
            for( int i = 0; i < len2; i++ )
            {
               var redirection = list2[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     redirection.Callback( context );

                     if( context.SkipRemainingPostfixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetBundleLoaded event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AssetBundleLoaded event." );
         }
         finally
         {
            _isFiringAssetBundle = false;
         }

         bundle = context.Bundle;
         return context;
      }

      internal static AsyncAssetBundleLoadingContext Hook_AssetBundleLoading_Prefix( AssetBundleLoadingParameters parameters, out AssetBundleCreateRequest request )
      {
         var context = new AsyncAssetBundleLoadingContext( parameters );

         if( _isFiringAssetBundle && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
         {
            request = null;
            return context;
         }

         try
         {
            _isFiringAssetBundle = true;

            if( _logAllLoadedResources )
            {
               XuaLogger.ResourceRedirector.Debug( $"Loading Asset Bundle (async): ({context.GetNormalizedPath()})." );
            }

            var list2 = PrefixRedirectionsForAsyncAssetBundles;
            var len2 = list2.Count;
            for( int i = 0; i < len2; i++ )
            {
               var redirection = list2[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     if( redirection.Callback is Action<AsyncAssetBundleLoadingContext> c1 )
                     {
                        c1( context );
                     }
                     else if( redirection.Callback is Action<IAssetBundleLoadingContext> c2 )
                     {
                        c2( context );
                     }

                     if( context.SkipRemainingPrefixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetBundleLoading event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AsyncAssetBundleLoading event." );
         }
         finally
         {
            _isFiringAssetBundle = false;
         }

         if( context.ResolveType == AsyncAssetBundleLoadingResolve.ThroughRequest )
         {
            request = context.Request;
         }
         else if( context.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle )
         {
            // simply create a dummy request object
            request = new AssetBundleCreateRequest(); // is this even legal?
            if( !context.SkipOriginalCall )
            {
               XuaLogger.ResourceRedirector.Warn( "Resolving sync over async asset load, but 'SkipOriginalCall' was not set to true. Forcing it to true." );
               context.SkipOriginalCall = true;
            }// Also, is there a nice way to replace the entire method with Harmony(X) (rather than just Prefix/Postfix) and obtain an reference to the original method that can be called as a part of it (like MonoMod detours/hooks)?
         }
         else
         {
            throw new InvalidOperationException( "Found invalid ResolveType on context: " + context.ResolveType );
         }

         return context;
      }

      internal static void Hook_AssetBundleLoading_Postfix( AsyncAssetBundleLoadingContext context, AssetBundleCreateRequest request )
      {
         if( request != null )
         {
            request.SetExtensionData( new AsyncAssetBundleLoadInfo(
               context.Parameters,
               context.Bundle,
               context.SkipAllPostfixes,
               context.ResolveType ) );
         }
      }

      internal static void Hook_AssetLoading_Postfix( AsyncAssetLoadingContext context, AssetBundleRequest request )
      {
         if( request != null )
         {
            request.SetExtensionData( new AsyncAssetLoadInfo(
               context.Parameters,
               context.Bundle,
               context.SkipAllPostfixes,
               context.ResolveType,
               context.Assets ) );
         }
      }

      internal static AssetLoadingContext Hook_AssetLoading_Prefix( AssetLoadingParameters parameters, AssetBundle parentBundle, ref UnityEngine.Object asset )
      {
#if MANAGED
         UnityEngine.Object[] arr = null;
#else
         Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> arr = null;
#endif


         var intention = Hook_AssetLoading_Prefix( parameters, parentBundle, ref arr );

         if( arr == null || arr.Length == 0 )
         {
            asset = null;
         }
         else if( arr.Length > 1 )
         {
            XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in AssetLoadeding event. Returned more than one asset to call requiring only a single asset." );
            asset = arr[ 0 ];
         }
         else if( arr.Length == 1 )
         {
            asset = arr[ 0 ];
         }

         return intention;
      }

#if MANAGED
      internal static AssetLoadingContext Hook_AssetLoading_Prefix( AssetLoadingParameters parameters, AssetBundle bundle, ref UnityEngine.Object[] assets )
#else
      internal static AssetLoadingContext Hook_AssetLoading_Prefix( AssetLoadingParameters parameters, AssetBundle bundle, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> assets )
#endif
      {
         var context = new AssetLoadingContext( parameters, bundle );
         try
         {
            if( _isFiringAsset && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
            {
               return context;
            }

            _isFiringAsset = true;

            var list1 = PrefixRedirectionsForAssetsPerCall;
            var len1 = list1.Count;
            for( int i = 0; i < len1; i++ )
            {
               var redirection = list1[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     if( redirection.Callback is Action<AssetLoadingContext> c1 )
                     {
                        c1( context );
                     }
                     else if( redirection.Callback is Action<IAssetLoadingContext> c2 )
                     {
                        c2( context );
                     }

                     if( context.SkipRemainingPrefixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoading event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }

            assets = context.Assets;
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AssetLoading event." );
         }
         finally
         {
            _isFiringAsset = false;
         }

         return context;
      }

      internal static AsyncAssetLoadingContext Hook_AsyncAssetLoading_Prefix( AssetLoadingParameters parameters, AssetBundle bundle, ref AssetBundleRequest request )
      {
         var context = new AsyncAssetLoadingContext( parameters, bundle );
         try
         {
            if( _isFiringAsset && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
            {
               return context;
            }

            _isFiringAsset = true;

            var list1 = PrefixRedirectionsForAsyncAssetsPerCall;
            var len1 = list1.Count;
            for( int i = 0; i < len1; i++ )
            {
               var redirection = list1[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     if( redirection.Callback is Action<AsyncAssetLoadingContext> c1 )
                     {
                        c1( context );
                     }
                     else if( redirection.Callback is Action<IAssetLoadingContext> c2 )
                     {
                        c2( context );
                     }

                     if( context.SkipRemainingPrefixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AsyncAssetLoading event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }

            if( context.ResolveType == AsyncAssetLoadingResolve.ThroughRequest )
            {
               request = context.Request;
            }
            else if( context.ResolveType == AsyncAssetLoadingResolve.ThroughAssets )
            {
               // simply create a dummy request object
               request = new AssetBundleRequest(); // is this even legal?
               if( !context.SkipOriginalCall )
               {
                  XuaLogger.ResourceRedirector.Warn( "Resolving sync over async asset load, but 'SkipOriginalCall' was not set to true. Forcing it to true." );
                  context.SkipOriginalCall = true;
               }
            }
            else
            {
               throw new InvalidOperationException( "Found invalid ResolveType on context: " + context.ResolveType );
            }
         }
         catch( Exception e )
         {
            XuaLogger.ResourceRedirector.Error( e, "An error occurred while invoking AsyncAssetLoading event." );
         }
         finally
         {
            _isFiringAsset = false;
         }

         return context;
      }

      internal static void Hook_AssetLoaded_Postfix( AssetLoadingParameters parameters, AssetBundle parentBundle, ref UnityEngine.Object asset )
      {
#if MANAGED
         UnityEngine.Object[] arr;
#else
         Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> arr;
#endif
         if( asset == null )
         {
            arr = new UnityEngine.Object[ 0 ];
         }
         else
         {
            arr = new[] { asset };
         }

         Hook_AssetLoaded_Postfix( parameters, parentBundle, ref arr );

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

#if MANAGED
      internal static void Hook_AssetLoaded_Postfix( AssetLoadingParameters parameters, AssetBundle bundle, ref UnityEngine.Object[] assets )
#else
      internal static void Hook_AssetLoaded_Postfix( AssetLoadingParameters parameters, AssetBundle bundle, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> assets )
#endif
      {
         FireAssetLoadedEvent( parameters.ToAssetLoadedParameters(), bundle, ref assets );
      }

      internal static void Hook_ResourceLoaded_Postfix( ResourceLoadedParameters parameters, ref UnityEngine.Object asset )
      {
#if MANAGED
         UnityEngine.Object[] arr;
#else
         Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> arr;
#endif
         if( asset == null )
         {
            arr = new UnityEngine.Object[ 0 ];
         }
         else
         {
            arr = new[] { asset };
         }

         Hook_ResourceLoaded_Postfix( parameters, ref arr );

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

#if MANAGED
      internal static void Hook_ResourceLoaded_Postfix( ResourceLoadedParameters parameters, ref UnityEngine.Object[] assets )
#else
      internal static void Hook_ResourceLoaded_Postfix( ResourceLoadedParameters parameters, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> assets )
#endif
      {
         FireResourceLoadedEvent( parameters, ref assets );
      }

#if MANAGED
      internal static void FireAssetLoadedEvent( AssetLoadedParameters parameters, AssetBundle assetBundle, ref UnityEngine.Object[] assets )
#else
      internal static void FireAssetLoadedEvent( AssetLoadedParameters parameters, AssetBundle assetBundle, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> assets )
#endif
      {
         var originalAssets = assets?.ToArray();
         try
         {
            var contextPerCall = new AssetLoadedContext( parameters, assetBundle, assets );

            if( _isFiringAsset && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
            {
               return;
            }

            _isFiringAsset = true;

            if( _logAllLoadedResources && assets != null )
            {
               for( int i = 0; i < assets.Length; i++ )
               {
                  var asset = assets[ i ];
                  if( asset != null )
                  {
                     var uniquePath = contextPerCall.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Loaded Asset: '{asset.GetUnityType().FullName}', Load Type: '{parameters.LoadType.ToString()}', Unique Path: ({uniquePath})." );
                  }
               }
            }

            // handle "per call" hooks first
            var list1 = PostfixRedirectionsForAssetsPerCall;
            var len1 = list1.Count;
            for( int i = 0; i < len1; i++ )
            {
               var redirection = list1[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     redirection.Callback( contextPerCall );

                     if( contextPerCall.SkipRemainingPostfixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }

            assets = contextPerCall.Assets;

            // handle "per resource" hooks afterwards
            if( !contextPerCall.SkipRemainingPostfixes && assets != null )
            {
               var len = assets.Length;
               for( int j = 0; j < len; j++ )
               {
                  var asset = assets[ j ];
                  if( asset != null )
                  {
                     var contextPerResource = new AssetLoadedContext( parameters, assetBundle, asset );

                     var list2 = PostfixRedirectionsForAssetsPerResource;
                     var len2 = list2.Count;
                     for( int i = 0; i < len2; i++ )
                     {
                        var redirection = list2[ i ];
                        if( !redirection.IsBeingCalled )
                        {
                           try
                           {
                              redirection.IsBeingCalled = true;
                              redirection.Callback( contextPerResource );

                              if( contextPerResource.Asset != null )
                              {
                                 assets[ j ] = contextPerResource.Asset;
                              }
                              else
                              {
                                 XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in AssetLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}." );
                              }

                              if( contextPerResource.SkipRemainingPostfixes ) break;
                           }
                           catch( Exception ex )
                           {
                              XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking AssetLoaded event." );
                           }
                           finally
                           {
                              RecursionEnabled = true;
                              redirection.IsBeingCalled = false;
                           }
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
            _isFiringAsset = false;

            if( originalAssets != null )
            {
               foreach( var asset in originalAssets )
               {
                  var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                  ext.HasBeenRedirected = true;
               }
            }
         }
      }

#if MANAGED
      internal static void FireResourceLoadedEvent( ResourceLoadedParameters parameters, ref UnityEngine.Object[] assets )
#else
      internal static void FireResourceLoadedEvent( ResourceLoadedParameters parameters, ref Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> assets )
#endif
      {
         var originalAssets = assets?.ToArray();
         try
         {
            var contextPerCall = new ResourceLoadedContext( parameters, assets );

            if( _isFiringResource && ( _isRecursionDisabledPermanently || !RecursionEnabled ) )
            {
               return;
            }

            _isFiringResource = true;

            if( _logAllLoadedResources && assets != null )
            {
               for( int i = 0; i < assets.Length; i++ )
               {
                  var asset = assets[ i ];
                  if( asset != null )
                  {
                     var uniquePath = contextPerCall.GetUniqueFileSystemAssetPath( asset );
                     XuaLogger.ResourceRedirector.Debug( $"Loaded Asset: '{asset.GetUnityType().FullName}', Load Type: '{parameters.LoadType.ToString()}', Unique Path: ({uniquePath})." );
                  }
               }
            }

            // handle "per call" hooks first
            var list1 = PostfixRedirectionsForResourcesPerCall;
            var len1 = list1.Count;
            for( int i = 0; i < len1; i++ )
            {
               var redirection = list1[ i ];
               if( !redirection.IsBeingCalled )
               {
                  try
                  {
                     redirection.IsBeingCalled = true;
                     redirection.Callback( contextPerCall );

                     if( contextPerCall.SkipRemainingPostfixes ) break;
                  }
                  catch( Exception ex )
                  {
                     XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking ResourceLoaded event." );
                  }
                  finally
                  {
                     RecursionEnabled = true;
                     redirection.IsBeingCalled = false;
                  }
               }
            }

            assets = contextPerCall.Assets;

            // handle "per resource" hooks afterwards
            if( !contextPerCall.SkipRemainingPostfixes && assets != null )
            {
               var len = assets.Length;
               for( int j = 0; j < len; j++ )
               {
                  var asset = assets[ j ];
                  if( asset != null )
                  {
                     var contextPerResource = new ResourceLoadedContext( parameters, asset );

                     var list2 = PostfixRedirectionsForResourcesPerResource;
                     var len2 = list2.Count;
                     for( int i = 0; i < len2; i++ )
                     {
                        var redirection = list2[ i ];
                        if( !redirection.IsBeingCalled )
                        {
                           try
                           {
                              redirection.IsBeingCalled = true;
                              redirection.Callback( contextPerResource );

                              if( contextPerResource.Asset != null )
                              {
                                 assets[ j ] = contextPerResource.Asset;
                              }
                              else
                              {
                                 XuaLogger.ResourceRedirector.Warn( $"Illegal behaviour by redirection handler in ResourceLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}." );
                              }

                              if( contextPerResource.SkipRemainingPostfixes ) break;
                           }
                           catch( Exception ex )
                           {
                              XuaLogger.ResourceRedirector.Error( ex, "An error occurred while invoking ResourceLoaded event." );
                           }
                           finally
                           {
                              RecursionEnabled = true;
                              redirection.IsBeingCalled = false;
                           }
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
            _isFiringResource = false;

            if( originalAssets != null )
            {
               foreach( var asset in originalAssets )
               {
                  var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
                  ext.HasBeenRedirected = true;
               }
            }
         }
      }

      private static void LogEventRegistration( string eventType, IEnumerable callbacks )
      {
         XuaLogger.ResourceRedirector.Debug( $"Registered new callback for {eventType}." );
         LogNewCallbackOrder( eventType, callbacks );
      }

      private static void LogEventUnregistration( string eventType, IEnumerable callbacks )
      {
         XuaLogger.ResourceRedirector.Debug( $"Unregistered callback for {eventType}." );
         LogNewCallbackOrder( eventType, callbacks );
      }

      private static void LogNewCallbackOrder( string eventType, IEnumerable callbacks )
      {
         if( !LogCallbackOrder ) return;

         XuaLogger.ResourceRedirector.Debug( $"New callback order for {eventType}:" );
         foreach( var redirection in callbacks )
         {
            XuaLogger.ResourceRedirector.Debug( redirection.ToString() );
         }
      }

      /// <summary>
      /// Register an AssetBundleLoaded hook (postfix to loading an asset bundle).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetBundleLoadedHook( int priority, Action<AssetBundleLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create( action, priority );
         if( PostfixRedirectionsForAssetBundles.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PostfixRedirectionsForAssetBundles.BinarySearchInsert( item );

         LogEventRegistration( "AssetBundleLoaded", PostfixRedirectionsForAssetBundles );
      }

      /// <summary>
      /// Register an AssetBundleLoaded hook (postfix to loading an asset bundle).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetBundleLoadedHook( Action<AssetBundleLoadedContext> action ) => RegisterAssetBundleLoadedHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AssetBundleLoaded hook (postfix to loading an asset bundle).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetBundleLoadedHook( Action<AssetBundleLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PostfixRedirectionsForAssetBundles.RemoveAll( x => Equals( x.Callback, action ) );

         LogEventUnregistration( "AssetBundleLoaded", PostfixRedirectionsForAssetBundles );
      }

      /// <summary>
      /// Register an AssetLoading hook (prefix to loading an asset from an asset bundle).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadingHook( int priority, Action<AssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAssetsPerCall.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAssetsPerCall.BinarySearchInsert( item );

         LogEventRegistration( "AssetLoading", PrefixRedirectionsForAssetsPerCall );
      }

      /// <summary>
      /// Register an AssetLoading hook (prefix to loading an asset from an asset bundle).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadingHook( Action<AssetLoadingContext> action ) => RegisterAssetLoadingHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AssetLoading hook (prefix to loading an asset from an asset bundle).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadingHook( Action<AssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAssetsPerCall.RemoveAll( x => Equals( x.Callback, action ) );

         LogEventUnregistration( "AssetLoading", PrefixRedirectionsForAssetsPerCall );
      }

      /// <summary>
      /// Register an AsyncAssetLoading hook (prefix to loading an asset from an asset bundle asynchronously).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAssetLoadingHook( int priority, Action<AsyncAssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAsyncAssetsPerCall.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAsyncAssetsPerCall.BinarySearchInsert( item );

         LogEventRegistration( "AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall );
      }

      /// <summary>
      /// Register an AsyncAssetLoading hook (prefix to loading an asset from an asset bundle asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAssetLoadingHook( Action<AsyncAssetLoadingContext> action ) => RegisterAsyncAssetLoadingHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AsyncAssetLoading hook (prefix to loading an asset from an asset bundle asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAssetLoadingHook( Action<AsyncAssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAsyncAssetsPerCall.RemoveAll( x => Equals( x.Callback, action ) );

         LogEventUnregistration( "AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall );
      }

      /// <summary>
      /// Register an AsyncAssetLoading hook and AssetLoading hook (prefix to loading an asset from an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAndSyncAssetLoadingHook( int priority, Action<IAssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAsyncAssetsPerCall.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAsyncAssetsPerCall.BinarySearchInsert( item );
         LogEventRegistration( "AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall );

         PrefixRedirectionsForAssetsPerCall.BinarySearchInsert( item );
         LogEventRegistration( "AssetLoading", PrefixRedirectionsForAssetsPerCall );
      }

      /// <summary>
      /// Register an AsyncAssetLoading hook and AssetLoading hook (prefix to loading an asset from an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAndSyncAssetLoadingHook( Action<IAssetLoadingContext> action ) => RegisterAsyncAndSyncAssetLoadingHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AsyncAssetLoading hook and AssetLoading hook (prefix to loading an asset from an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAndSyncAssetLoadingHook( Action<IAssetLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAsyncAssetsPerCall.RemoveAll( x => Equals( x.Callback, action ) );
         LogEventUnregistration( "AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall );

         PrefixRedirectionsForAssetsPerCall.RemoveAll( x => Equals( x.Callback, action ) );
         LogEventUnregistration( "AssetLoading", PrefixRedirectionsForAssetsPerCall );
      }

      /// <summary>
      /// Register an AssetLoaded hook (postfix to loading an asset from an asset bundle (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadedHook( HookBehaviour behaviour, int priority, Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create( action, priority );
         if( PostfixRedirectionsForAssetsPerCall.Contains( item )
            || PostfixRedirectionsForAssetsPerResource.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            PostfixRedirectionsForAssetsPerCall.BinarySearchInsert( item );

            LogEventRegistration( $"AssetLoaded ({behaviour.ToString()})", PostfixRedirectionsForAssetsPerCall );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            PostfixRedirectionsForAssetsPerResource.BinarySearchInsert( item );

            LogEventRegistration( $"AssetLoaded ({behaviour.ToString()})", PostfixRedirectionsForAssetsPerResource );
         }
      }

      /// <summary>
      /// Register an AssetLoaded hook (postfix to loading an asset from an asset bundle (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetLoadedHook( HookBehaviour behaviour, Action<AssetLoadedContext> action ) => RegisterAssetLoadedHook( behaviour, CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AssetLoaded hook (postfix to loading an asset from an asset bundle (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetLoadedHook( Action<AssetLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var c1 = PostfixRedirectionsForAssetsPerCall.RemoveAll( x => x.Callback == action );
         if( c1 > 0 )
         {
            LogEventRegistration( $"AssetLoaded ({HookBehaviour.OneCallbackPerLoadCall.ToString()})", PostfixRedirectionsForAssetsPerCall );
         }

         var c2 = PostfixRedirectionsForAssetsPerResource.RemoveAll( x => x.Callback == action );
         if( c2 > 0 )
         {
            LogEventRegistration( $"AssetLoaded ({HookBehaviour.OneCallbackPerResourceLoaded.ToString()})", PostfixRedirectionsForAssetsPerResource );
         }
      }

      /// <summary>
      /// Register an AssetBundleLoading hook (prefix to loading an asset bundle synchronously).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetBundleLoadingHook( int priority, Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAssetBundles.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAssetBundles.BinarySearchInsert( item );

         LogEventRegistration( $"AssetBundleLoading", PrefixRedirectionsForAssetBundles );
      }

      /// <summary>
      /// Register an AssetBundleLoading hook (prefix to loading an asset bundle synchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAssetBundleLoadingHook( Action<AssetBundleLoadingContext> action ) => RegisterAssetBundleLoadingHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AssetBundleLoading hook (prefix to loading an asset bundle synchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAssetBundleLoadingHook( Action<AssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAssetBundles.RemoveAll( x => Equals( x.Callback, action ) );

         LogEventUnregistration( $"AssetBundleLoading", PrefixRedirectionsForAssetBundles );
      }

      /// <summary>
      /// Register an AsyncAssetBundleLoading hook (prefix to loading an asset bundle asynchronously).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAssetBundleLoadingHook( int priority, Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAsyncAssetBundles.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAsyncAssetBundles.BinarySearchInsert( item );

         LogEventRegistration( $"AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles );
      }

      /// <summary>
      /// Register an AsyncAssetBundleLoading hook (prefix to loading an asset bundle asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAssetBundleLoadingHook( Action<AsyncAssetBundleLoadingContext> action ) => RegisterAsyncAssetBundleLoadingHook( CallbackPriority.Default, action );

      /// <summary>
      /// Unregister an AsyncAssetBundleLoading hook (prefix to loading an asset bundle asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAssetBundleLoadingHook( Action<AsyncAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAsyncAssetBundles.RemoveAll( x => Equals( x.Callback, action ) );

         LogEventUnregistration( $"AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles );
      }

      /// <summary>
      /// Register an AsyncAssetBundleLoading hook and AssetBundleLoading hook (prefix to loading an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAndSyncAssetBundleLoadingHook( int priority, Action<IAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create<Delegate>( action, priority );
         if( PrefixRedirectionsForAssetBundles.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         PrefixRedirectionsForAssetBundles.BinarySearchInsert( item );
         LogEventRegistration( $"AssetBundleLoading", PrefixRedirectionsForAssetBundles );

         PrefixRedirectionsForAsyncAssetBundles.BinarySearchInsert( item );
         LogEventRegistration( $"AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles );
      }

      /// <summary>
      /// Register an AsyncAssetBundleLoading hook and AssetBundleLoading hook (prefix to loading an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void RegisterAsyncAndSyncAssetBundleLoadingHook( Action<IAssetBundleLoadingContext> action ) => RegisterAsyncAndSyncAssetBundleLoadingHook( CallbackPriority.Default, action );


      /// <summary>
      /// Unregister an AsyncAssetBundleLoading hook and AssetBundleLoading hook (prefix to loading an asset bundle synchronously/asynchronously).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterAsyncAndSyncAssetBundleLoadingHook( Action<IAssetBundleLoadingContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         PrefixRedirectionsForAssetBundles.RemoveAll( x => Equals( x.Callback, action ) );
         LogEventUnregistration( $"AssetBundleLoading", PrefixRedirectionsForAssetBundles );

         PrefixRedirectionsForAsyncAssetBundles.RemoveAll( x => Equals( x.Callback, action ) );
         LogEventUnregistration( $"AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles );
      }

      /// <summary>
      /// Register a ResourceLoaded hook (postfix to loading a resource from the Resources API (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="priority">The priority of the callback, the higher the sooner it will be called.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterResourceLoadedHook( HookBehaviour behaviour, int priority, Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var item = PrioritizedCallback.Create( action, priority );
         if( PostfixRedirectionsForResourcesPerCall.Contains( item )
            || PostfixRedirectionsForResourcesPerResource.Contains( item ) )
         {
            throw new ArgumentException( "This callback has already been registered.", "action" );
         }

         Initialize();

         if( behaviour == HookBehaviour.OneCallbackPerLoadCall )
         {
            PostfixRedirectionsForResourcesPerCall.BinarySearchInsert( item );

            LogEventRegistration( $"ResourceLoaded ({behaviour.ToString()})", PostfixRedirectionsForResourcesPerCall );
         }
         else if( behaviour == HookBehaviour.OneCallbackPerResourceLoaded )
         {
            PostfixRedirectionsForResourcesPerResource.BinarySearchInsert( item );

            LogEventRegistration( $"ResourceLoaded ({behaviour.ToString()})", PostfixRedirectionsForResourcesPerResource );
         }
      }

      /// <summary>
      /// Register a ResourceLoaded hook (postfix to loading a resource from the Resources API (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="behaviour">The behaviour of the callback.</param>
      /// <param name="action">The callback.</param>
      public static void RegisterResourceLoadedHook( HookBehaviour behaviour, Action<ResourceLoadedContext> action ) => RegisterResourceLoadedHook( behaviour, CallbackPriority.Default, action );

      /// <summary>
      /// Unregister a ReourceLoaded hook (postfix to loading a resource from the Resources API (both synchronous and asynchronous)).
      /// </summary>
      /// <param name="action">The callback.</param>
      public static void UnregisterResourceLoadedHook( Action<ResourceLoadedContext> action )
      {
         if( action == null ) throw new ArgumentNullException( "action" );

         var c1 = PostfixRedirectionsForResourcesPerCall.RemoveAll( x => x.Callback == action );
         if( c1 > 0 )
         {
            LogEventRegistration( $"ResourceLoaded ({HookBehaviour.OneCallbackPerLoadCall.ToString()})", PostfixRedirectionsForResourcesPerCall );
         }

         var c2 = PostfixRedirectionsForResourcesPerResource.RemoveAll( x => x.Callback == action );
         if( c2 > 0 )
         {
            LogEventRegistration( $"ResourceLoaded ({HookBehaviour.OneCallbackPerResourceLoaded.ToString()})", PostfixRedirectionsForResourcesPerResource );
         }
      }
   }
}
