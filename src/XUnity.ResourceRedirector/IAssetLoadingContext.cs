using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AssetLoading hook (synchronous) or the AsyncAssetLoading hook (asynchronous).
   /// </summary>
   public interface IAssetLoadingContext
   {
      /// <summary>
      /// Gets the original path the asset bundle was loaded with.
      /// </summary>
      /// <returns>The unmodified, original path the asset bundle was loaded with.</returns>
      string GetAssetBundlePath();

      /// <summary>
      /// Gets the normalized path to the asset bundle that is:
      ///  * Relative to the current directory
      ///  * Lower-casing
      ///  * Uses '\' as separators.
      /// </summary>
      /// <returns></returns>
      string GetNormalizedAssetBundlePath();

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset/resource load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the asset, you likely want to set this to true.</param>
      /// <param name="skipAllPostfixes">Indicate if the postfixes should be skipped.</param>
      void Complete( bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true );

      /// <summary>
      /// Disables recursive calls if you make an asset/asset bundle load call
      /// from within your callback. If you want to prevent recursion this should
      /// be called before you load the asset/asset bundle.
      /// </summary>
      void DisableRecursion();

      /// <summary>
      /// Gets the parameters the asset load call was called with.
      /// </summary>
      AssetLoadingParameters Parameters { get; }

      /// <summary>
      /// Gets the AssetBundle associated with the loaded assets.
      /// </summary>
      AssetBundle Bundle { get; }

      /// <summary>
      /// Gets or sets the loaded assets.
      ///
      /// Consider using this if the load type is 'LoadByType' or 'LoadNamedWithSubAssets'.
      /// </summary>
#if MANAGED
      UnityEngine.Object[] Assets { get; set; }
#else
      Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> Assets { get; set; }
#endif

      /// <summary>
      /// Gets or sets the loaded assets. This is simply equal to the first index of the Assets property, with some
      /// additional null guards to prevent NullReferenceExceptions when using it.
      /// </summary>
      UnityEngine.Object Asset { get; set; }
   }
}
