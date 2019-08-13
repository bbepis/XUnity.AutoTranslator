using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   internal class RedirectionContext<TAsset> : IRedirectionContext<TAsset>
      where TAsset : UnityEngine.Object
   {
      public RedirectionContext( string uniqueAssetId, TAsset asset )
      {
         UniqueAssetId = uniqueAssetId;
         Asset = asset;
         Handled = false;
      }

      public string UniqueAssetId { get; }

      public TAsset Asset { get; set; }

      public bool Handled { get; set; }
   }
}
