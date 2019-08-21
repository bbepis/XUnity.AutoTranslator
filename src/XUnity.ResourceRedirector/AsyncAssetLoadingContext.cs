using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AsyncAssetLoading hook (asynchronous).
   /// </summary>
   public class AsyncAssetLoadingContext : IAssetLoadingContext
   {
      internal AsyncAssetLoadingContext( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle )
      {
         OriginalParameters = new AssetLoadParameters( assetName, assetType, loadType );
         Bundle = bundle;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset/resource load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the asset, you likely want to set this to true.</param>
      /// <param name="skipAllPostfixes">Indicate if the postfixes should be skipped.</param>
      public void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = null, bool? skipAllPostfixes = null )
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
      /// Gets the original parameters the asset load call was called with.
      /// </summary>
      public AssetLoadParameters OriginalParameters { get; }

      /// <summary>
      /// Gets the AssetBundle associated with the loaded assets.
      /// </summary>
      public AssetBundle Bundle { get; }

      /// <summary>
      /// Gets or sets the AssetBundleRequest used to load assets.
      /// </summary>
      public AssetBundleRequest Request { get; set; }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; private set; }

      internal bool SkipAllPostfixes { get; private set; }
   }
}
