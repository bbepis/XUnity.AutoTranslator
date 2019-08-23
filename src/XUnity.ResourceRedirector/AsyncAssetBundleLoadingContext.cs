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

      internal AsyncAssetBundleLoadingContext( string assetBundlePath, uint crc, ulong offset, AssetBundleLoadType loadType )
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
               .MakeRelativePath( EnvironmentEx.LoweredCurrentDirectory );
         }
         return _normalizedPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset bundle load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the request, you likely want to set this to true.</param>
      public void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = true )
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
      /// Gets or sets the AssetBundleCreateRequest being used to load the AssetBundle.
      /// </summary>
      public AssetBundleCreateRequest Request { get; set; }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; private set; }
   }
}
