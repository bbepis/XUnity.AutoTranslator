using UnityEngine;
using XUnity.Common.Extensions;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the asset bundle loading event when loaded
   /// through the async API.
   /// </summary>
   public class AsyncAssetBundleLoadingContext
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
               .MakeRelativePath( Constants.LoweredCurrentDirectory );
         }
         return _normalizedPath;
      }

      /// <summary>
      /// Gets the parameters of the original call.
      /// </summary>
      public AssetBundleLoadParameters OriginalParameters { get; set; }

      /// <summary>
      /// Gets or sets the AssetBundleCreateRequest being used to load the AssetBundle.
      /// </summary>
      public AssetBundleCreateRequest Request { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      public bool Handled { get; set; }
   }
}
