using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the asset bundle loading event.
   /// </summary>
   public class AssetBundleLoadingContext
   {
      internal AssetBundleLoadingContext( string assetBundlePath, string normalizedAssetBundlePath )
      {
         AssetBundlePath = assetBundlePath;
         NormalizedAssetBundlePath = normalizedAssetBundlePath;
      }

      /// <summary>
      /// Gets the path of the asset bundle.
      /// </summary>
      public string AssetBundlePath { get; } // relative? absolute? Could make relative!

      /// <summary>
      /// Gets the normalized path of the asset bundle.
      /// </summary>
      public string NormalizedAssetBundlePath { get; }

      /// <summary>
      /// Gets or sets the AssetBundle being loaded.
      /// </summary>
      public AssetBundle Bundle { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      public bool Handled { get; set; }
   }
}
