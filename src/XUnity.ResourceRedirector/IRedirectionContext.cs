using System.Collections.Generic;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   public interface IRedirectionContext
   {
      /// <summary>
      /// Gets a globally unique path for the resource being loaded, combined of the source, path and asset name. Uses '\' for seperators.
      /// </summary>
      string UniqueFileSystemAssetPath { get; }

      /// <summary>
      /// Gets a unique path of the resource being loaded. Uses '/' for seperators.
      /// </summary>
      string AssetPath { get; }

      /// <summary>
      /// Gets the bundle this asset is associated with. May be null if loaded through Resources API.
      /// </summary>
      AssetBundle Bundle { get; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      bool Handled { get; set; }

      /// <summary>
      /// Gets or sets (to be overriden) the asset being loaded.
      /// </summary>
      UnityEngine.Object Asset { get; set; }

      /// <summary>
      /// Gets the source of the asset - whether or not the call
      /// is coming from the Resources or AssetBundle API.
      /// </summary>
      AssetSource Source { get; }

      /// <summary>
      /// Gets a bool indicating if this resource has been redirected before.
      /// </summary>
      bool HasReferenceBeenRedirectedBefore { get; }
   }

   /// <summary>
   /// Information related to the OnAssetLoading event.
   /// </summary>
   /// <typeparam name="TAsset">Is the asset being loaded.</typeparam>
   public interface IRedirectionContext<TAsset> : IRedirectionContext
   {
      /// <summary>
      /// Gets or sets (to be overriden) the asset being loaded.
      /// </summary>
      new TAsset Asset { get; set; }
   }
}
