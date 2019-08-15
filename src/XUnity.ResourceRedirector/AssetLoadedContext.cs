using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the asset loaded event.
   /// </summary>
   public abstract class AssetLoadedContext
   {
      private string _globallyUniqueAssetPath;

      internal AssetLoadedContext( AssetSource source, string assetPath, AssetBundle bundle, UnityEngine.Object asset, bool hasBeenRedirectedBefore )
      {
         Source = source;
         AssetPath = assetPath;
         Bundle = bundle;
         Asset = asset;
         HasReferenceBeenRedirectedBefore = hasBeenRedirectedBefore;
         Handled = false;
      }

      /// <summary>
      /// Gets generated path that can uniquely identity the resources across the two APIs.
      /// </summary>
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

      /// <summary>
      /// Gets the path of the asset.
      /// For the Resources API, this is the everything but the last segment of the resource.
      /// For the Assetbundle API, this is the name of the asset bundle.
      /// </summary>
      public string AssetPath { get; }

      /// <summary>
      /// Gets the AssetBundle if the asset is being loaded through the AssetBundle API, otherwise null.
      /// </summary>
      public AssetBundle Bundle { get; }

      /// <summary>
      /// Gets the source of the asset.
      /// </summary>
      public AssetSource Source { get; }

      /// <summary>
      /// Gets or sets the asset. Set it to override.
      /// </summary>
      public UnityEngine.Object Asset { get; set; }

      /// <summary>
      /// Gets a bool indicating if this resource has already been redirected before.
      /// </summary>
      public bool HasReferenceBeenRedirectedBefore { get; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      public bool Handled { get; set; }

   }

   /// <summary>
   /// The operation context surrounding the asset loaded event.
   /// </summary>
   public class AssetLoadedContext<TAsset> : AssetLoadedContext
      where TAsset : UnityEngine.Object
   {
      internal AssetLoadedContext( AssetSource source, string assetPath, AssetBundle bundle, TAsset asset, bool hasBeenRedirectedBefore ) : base( source, assetPath, bundle, asset, hasBeenRedirectedBefore )
      {
      }

      /// <summary>
      /// Gets or sets the asset. Set it to override.
      /// </summary>
      public new TAsset Asset
      {
         get => (TAsset)base.Asset;
         set => base.Asset = value;
      }
   }
}
