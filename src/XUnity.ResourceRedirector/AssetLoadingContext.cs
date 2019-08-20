using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AssetLoading hook (synchronous).
   /// </summary>
   public class AssetLoadingContext
   {
      internal AssetLoadingContext( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle )
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
      /// Gets or sets the loaded assets.
      ///
      /// Consider using this if the load type is 'LoadByType' or 'LoadNamedWithSubAssets'.
      /// </summary>
      public UnityEngine.Object[] Assets { get; set; }

      /// <summary>
      /// Gets or sets the loaded assets. This is simply equal to the first index of the Assets property, with some
      /// additional null guards to prevent NullReferenceExceptions when using it.
      /// </summary>
      public UnityEngine.Object Asset
      {
         get
         {
            if( Assets == null || Assets.Length < 1 )
            {
               return null;
            }
            return Assets[ 0 ];
         }
         set
         {
            if( Assets == null || Assets.Length < 1 )
            {
               Assets = new UnityEngine.Object[ 1 ];
            }
            Assets[ 0 ] = value;
         }
      }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; private set; }

      internal bool SkipAllPostfixes { get; private set; }
   }
}
