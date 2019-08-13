using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   /// <summary>
   /// Information related to the OnAssetLoading event.
   /// </summary>
   /// <typeparam name="TAsset">Is the asset being loaded.</typeparam>
   public interface IRedirectionContext<TAsset>
   {
      /// <summary>
      /// Gets a unique path of the resource being loaded.
      /// </summary>
      string UniqueAssetId { get; }

      /// <summary>
      /// Gets or sets (to be overriden) the asset being loaded.
      /// </summary>
      TAsset Asset { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      bool Handled { get; set; }


      // More contextual information
      // AssetBundleName?
      // ASsetName?
      // manifest crap?
   }
}
