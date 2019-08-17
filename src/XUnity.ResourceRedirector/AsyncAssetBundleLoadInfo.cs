using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class AsyncAssetBundleLoadInfo
   {
      public AsyncAssetBundleLoadInfo( string assetName, Type assetType, AssetLoadType loadType, AssetBundle bundle )
      {
         AssetName = assetName;
         AssetType = assetType;
         LoadType = loadType;
         Bundle = bundle;
      }

      public string AssetName { get; }
      public Type AssetType { get; }
      public AssetLoadType LoadType { get; }
      public AssetBundle Bundle { get; }
   }
}
