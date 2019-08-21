using System;
using UnityEngine;
using XUnity.Common.Extensions;

namespace XUnity.ResourceRedirector
{

   /// <summary>
   /// The operation context surrounding the AssetBundleLoading hook (synchronous).
   /// </summary>
   public class AssetBundleLoadingContext : IAssetBundleLoadingContext
   {
      private string _normalizedPath;

      internal AssetBundleLoadingContext( string assetBundlePath, uint crc, ulong offset, AssetBundleLoadType loadType )
      {
         OriginalParameters = new AssetBundleLoadParameters( assetBundlePath, crc, offset, loadType );
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
         if( _normalizedPath == null && OriginalParameters.Path != null )
         {
            _normalizedPath = OriginalParameters.Path
               .ToLowerInvariant()
               .Replace( '/', '\\' )
               .MakeRelativePath( Constants.LoweredCurrentDirectory );
         }
         return _normalizedPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the asset bundle, you likely want to set this to true.</param>
      public void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = null )
      {
         SkipRemainingPrefixes = skipRemainingPrefixes;
         if( skipOriginalCall.HasValue )
         {
            SkipOriginalCall = skipOriginalCall.Value;
         }
      }

      /// <summary>
      /// Gets the parameters of the original call.
      /// </summary>
      public AssetBundleLoadParameters OriginalParameters { get; }

      /// <summary>
      /// Gets or sets the AssetBundle being loaded.
      /// </summary>
      public AssetBundle Bundle { get; set; }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; private set; }
   }
}
