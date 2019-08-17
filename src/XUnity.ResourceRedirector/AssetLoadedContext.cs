using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// The operation context surrounding the asset loaded event.
   /// </summary>
   public class AssetLoadedContext : IAssetOrResourceLoadedContext
   {
      private bool? _hasBeenRedirectedBefore;
      private string _uniqueFileSystemAssetPath;

      internal AssetLoadedContext( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle, UnityEngine.Object[] assets )
      {
         OriginalParameters = new AssetLoadParameters( assetName, assetType, loadType );
         Bundle = bundle;
         Assets = assets;
         Handled = false;
      }

      /// <summary>
      /// Gets a bool indicating if this resource has already been redirected before.
      /// </summary>
      public bool HasReferenceBeenRedirectedBefore( UnityEngine.Object asset )
      {
         if( !_hasBeenRedirectedBefore.HasValue )
         {
            var ext = asset.GetOrCreateExtensionData<ResourceExtensionData>();
            _hasBeenRedirectedBefore = ext.HasBeenRedirected;
         }
         return _hasBeenRedirectedBefore.Value;
      }

      /// <summary>
      /// Gets a file system path for the specfic asset that should be unique.
      /// </summary>
      /// <param name="asset"></param>
      /// <returns></returns>
      public string GetUniqueFileSystemAssetPath( UnityEngine.Object asset )
      {
         // TODO: Optimize with StringBuilder
         if( _uniqueFileSystemAssetPath == null )
         {
            string path = "assets";

            var assetBundleName = Bundle.name;
            if( !string.IsNullOrEmpty( assetBundleName ) )
            {
               path = Path.Combine( path, assetBundleName.ToLowerInvariant() );
            }
            else
            {
               path = Path.Combine( path, "unnamed_assetbundle" );
            }

            var assetName = asset.name;
            if( !string.IsNullOrEmpty( assetName ) )
            {
               path = Path.Combine( path, assetName.ToLowerInvariant() );
            }
            else
            {
               string suffix = null;
               if( Assets.Length > 1 )
               {
                  var idx = Array.IndexOf( Assets, asset );
                  if( idx == -1 )
                  {
                     suffix = "_with_unknown_index";
                  }
                  else
                  {
                     suffix = "_" + idx.ToString( CultureInfo.InvariantCulture );
                  }
               }

               path = Path.Combine( path, OriginalParameters.LoadType == AssetLoadType.LoadMainAsset ? "main_asset" : "unnamed_asset" + suffix );
            }

            path = path.Replace( '/', '\\' );

            _uniqueFileSystemAssetPath = path;
         }

         return _uniqueFileSystemAssetPath;
      }

      /// <summary>
      /// Gets the original parameters the asset load call was called with.
      /// </summary>
      public AssetLoadParameters OriginalParameters { get; }

      /// <summary>
      /// Gets the AssetBundle associated with the loaded assets.
      /// </summary>
      public AssetBundle Bundle { get; }

      /// <summary>
      /// Gets the loaded assets. Override individual indices to change the asset reference that will be loaded.
      /// </summary>
      public UnityEngine.Object[] Assets { get; }

      /// <summary>
      /// Gets or sets a bool indicating if this event has been handled. Setting
      /// this will cause it to no longer propagate.
      /// </summary>
      public bool Handled { get; set; }
   }
}
