using System;
using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AsyncAssetBundleLoading hook (asynchronous).
   /// </summary>
   public class AsyncAssetBundleLoadingContext : IAssetBundleLoadingContext
   {
      private string _normalizedPath;
      private AssetBundle _bundle;
      private AssetBundleCreateRequest _request;

      internal AsyncAssetBundleLoadingContext( AssetBundleLoadingParameters parameters )
      {
         Parameters = parameters;
      }

      /// <summary>
      /// Gets a normalized path to the asset bundle that is:
      ///  * Relative to the current directory
      ///  * Lower-casing
      ///  * Uses '\' as separators.
      /// </summary>
      /// <returns></returns>
      public string GetNormalizedPath()
      {
         if( _normalizedPath == null && Parameters.Path != null )
         {
            _normalizedPath = Parameters.Path
               .ToLowerInvariant()
               .UseCorrectDirectorySeparators()
               .MakeRelativePath( EnvironmentEx.LoweredCurrentDirectory );
         }
         return _normalizedPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      public void Complete()
      {
         Complete( true, true, true );
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the request, you likely want to set this to true.</param>
      public void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = true )
      {
         Complete( skipRemainingPrefixes, skipOriginalCall, true );
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the request, you likely want to set this to true.</param>
      /// <param name="skipAllPostfixes">Indicate if all the postfixes should be skipped.</param>
      public void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true )
      {
         SkipRemainingPrefixes = skipRemainingPrefixes;
         if( skipOriginalCall.HasValue )
         {
            SkipOriginalCall = skipOriginalCall.Value;
         }
         if( skipAllPostfixes.HasValue )
         {
            SkipAllPostfixes = skipAllPostfixes.Value;
         }
      }

      /// <summary>
      /// Disables recursive calls if you make an asset/asset bundle load call
      /// from within your callback. If you want to prevent recursion this should
      /// be called before you load the asset/asset bundle.
      /// </summary>
      public void DisableRecursion()
      {
         ResourceRedirection.RecursionEnabled = false;
      }

      /// <summary>
      /// Gets the parameters of the call.
      /// </summary>
      public AssetBundleLoadingParameters Parameters { get; }

      /// <summary>
      /// Gets or sets the AssetBundleCreateRequest being used to load the AssetBundle.
      /// </summary>
      public AssetBundleCreateRequest Request
      {
         get
         {
            return _request;
         }
         set
         {
            _request = value;
            ResolveType = AsyncAssetBundleLoadingResolve.ThroughRequest;
         }
      }

      /// <summary>
      /// Gets or sets the AssetBundle being loaded.
      /// </summary>
      public AssetBundle Bundle
      {
         get
         {
            return _bundle;
         }
         set
         {
            if( !ResourceRedirection.SyncOverAsyncEnabled )
            {
               throw new InvalidOperationException( "Trying to set the Bundle property in async load operation while 'SyncOverAsyncAssetLoads' is disabled is not allowed. Consider settting the Request property instead if possible or enabling 'SyncOverAsyncAssetLoads' through the method 'ResourceRedirection.EnableSyncOverAsyncAssetLoads()'." );
            }

            _bundle = value;
            ResolveType = AsyncAssetBundleLoadingResolve.ThroughBundle;
         }
      }

      /// <summary>
      /// Gets or sets how this load operation should be resolved.
      /// Setting the Bundle/Request property will automatically update this value.
      /// </summary>
      public AsyncAssetBundleLoadingResolve ResolveType { get; set; }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; set; }

      internal bool SkipAllPostfixes { get; private set; }
   }
}
