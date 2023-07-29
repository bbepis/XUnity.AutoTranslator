using System;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the AssetLoading hook (synchronous).
   /// </summary>
   public class AssetLoadingContext : IAssetLoadingContext
   {
      private AssetBundleExtensionData _ext;
      private bool _lookedForExt = false;
      private BackingFieldOrArray _backingField;

      internal AssetLoadingContext( AssetLoadingParameters parameters, AssetBundle bundle )
      {
         Parameters = parameters;
         Bundle = bundle;
      }

      /// <summary>
      /// Gets the original path the asset bundle was loaded with.
      /// </summary>
      /// <returns>The unmodified, original path the asset bundle was loaded with.</returns>
      public string GetAssetBundlePath()
      {
         if( !_lookedForExt )
         {
            _lookedForExt = true;
            _ext = Bundle.GetExtensionData<AssetBundleExtensionData>();
         }

         return _ext?.Path;
      }

      /// <summary>
      /// Gets the normalized path to the asset bundle that is:
      ///  * Relative to the current directory
      ///  * Lower-casing
      ///  * Uses '\' as separators.
      /// </summary>
      /// <returns></returns>
      public string GetNormalizedAssetBundlePath()
      {
         if( !_lookedForExt )
         {
            _lookedForExt = true;
            _ext = Bundle.GetExtensionData<AssetBundleExtensionData>();
         }

         return _ext?.NormalizedPath;
      }

      /// <summary>
      /// Indicate your work is done and if any other hooks to this asset/resource load should be called.
      /// </summary>
      /// <param name="skipRemainingPrefixes">Indicate if the remaining prefixes should be skipped.</param>
      /// <param name="skipOriginalCall">Indicate if the original call should be skipped. If you set the asset, you likely want to set this to true.</param>
      /// <param name="skipAllPostfixes">Indicate if the postfixes should be skipped.</param>
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
      /// Gets the parameters the asset load call was called with.
      /// </summary>
      public AssetLoadingParameters Parameters { get; }

      /// <summary>
      /// Gets the AssetBundle associated with the loaded assets.
      /// </summary>
      public AssetBundle Bundle { get; }

      /// <summary>
      /// Gets or sets the loaded assets.
      ///
      /// Consider using this if the load type is 'LoadByType' or 'LoadNamedWithSubAssets'.
      /// </summary>
#if MANAGED
      public UnityEngine.Object[] Assets { get => _backingField.Array; set => _backingField.Array = value; }
#else
      public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> Assets { get => _backingField.Array; set => _backingField.Array = value; }
#endif

      /// <summary>
      /// Gets or sets the loaded assets. This is simply equal to the first index of the Assets property, with some
      /// additional null guards to prevent NullReferenceExceptions when using it.
      /// </summary>
      public UnityEngine.Object Asset { get => _backingField.Field; set => _backingField.Field = value; }

      internal bool SkipRemainingPrefixes { get; private set; }

      internal bool SkipOriginalCall { get; private set; }

      internal bool SkipAllPostfixes { get; private set; }
   }
}
