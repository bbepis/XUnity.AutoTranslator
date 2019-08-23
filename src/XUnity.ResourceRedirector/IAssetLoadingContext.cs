using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AssetLoading hook (synchronous) or the AsyncAssetLoading hook (asynchronous).
   /// </summary>
   public interface IAssetLoadingContext
   {
      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset/resource load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the asset, you likely want to set this to true.</param>
      /// <param name="skipAllPostfixes">Indicate if the postfixes should be skipped.</param>
      void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true );

      /// <summary>
      /// Gets the original parameters the asset load call was called with.
      /// </summary>
      AssetLoadParameters OriginalParameters { get; }

      /// <summary>
      /// Gets the AssetBundle associated with the loaded assets.
      /// </summary>
      AssetBundle Bundle { get; }
   }
}
