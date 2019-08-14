using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class RedirectionContext<TAsset> : IRedirectionContext<TAsset>
      where TAsset : UnityEngine.Object
   {
      private string _globallyUniqueAssetPath;

      public RedirectionContext( AssetSource source, string assetPath, TAsset asset, bool hasBeenRedirectedBefore )
      {
         Source = source;
         AssetPath = assetPath;
         Asset = asset;
         HasBeenRedirectedBefore = hasBeenRedirectedBefore;
         Handled = false;
      }

      public string AssetPath { get; }

      public string GloballyUniqueAssetPath
      {
         get
         {
            if( _globallyUniqueAssetPath == null )
            {
               _globallyUniqueAssetPath = Path.Combine(
                  Source == AssetSource.AssetBundle ? "assets" : "resources",
                  AssetPath );
            }
            return _globallyUniqueAssetPath;
         }
      }

      public AssetSource Source { get; }

      public TAsset Asset { get; set; }

      public bool HasBeenRedirectedBefore { get; }

      public bool Handled { get; set; }

      Object IRedirectionContext.Asset
      {
         get => Asset;
         set => Asset = (TAsset)value;
      }
   }
}
