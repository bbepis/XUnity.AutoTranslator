using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class RedirectionContext<TAsset> : IRedirectionContext<TAsset>
      where TAsset : UnityEngine.Object
   {
      private string _globallyUniqueAssetPath;

      public RedirectionContext( AssetSource source, string assetPath, AssetBundle bundle, TAsset asset, bool hasBeenRedirectedBefore )
      {
         Source = source;
         AssetPath = assetPath;
         Bundle = bundle;
         Asset = asset;
         HasReferenceBeenRedirectedBefore = hasBeenRedirectedBefore;
         Handled = false;
      }

      public string AssetPath { get; }

      public AssetBundle Bundle { get; }

      public string UniqueFileSystemAssetPath
      {
         get
         {
            if( _globallyUniqueAssetPath == null )
            {
               var path = Path.Combine(
                  Source == AssetSource.AssetBundle ? "assets" : "resources",
                  AssetPath );

               if( string.IsNullOrEmpty( Asset.name ) )
               {
                  _globallyUniqueAssetPath = path;
               }
               else
               {
                  _globallyUniqueAssetPath = Path.Combine( path, Asset.name ).ToLowerInvariant();
               }

               _globallyUniqueAssetPath = _globallyUniqueAssetPath.Replace( '/', '\\' );
            }
            return _globallyUniqueAssetPath;
         }
      }

      public AssetSource Source { get; }

      public TAsset Asset { get; set; }

      public bool HasReferenceBeenRedirectedBefore { get; }

      public bool Handled { get; set; }

      Object IRedirectionContext.Asset
      {
         get => Asset;
         set => Asset = (TAsset)value;
      }
   }
}
