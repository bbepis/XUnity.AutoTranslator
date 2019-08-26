using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class AsyncAssetLoadInfo
   {
      public AsyncAssetLoadInfo(
         string assetName,
         Type assetType,
         AssetLoadType loadType,
         AssetBundle bundle,
         bool skipAllPostfixes,
         AsyncAssetLoadingResolve resolveType,
         UnityEngine.Object[] assets )
      {
         AssetName = assetName;
         AssetType = assetType;
         LoadType = loadType;
         Bundle = bundle;
         SkipAllPostfixes = skipAllPostfixes;
         ResolveType = resolveType;
         Assets = assets;
      }

      public string AssetName { get; }
      public Type AssetType { get; }
      public AssetLoadType LoadType { get; }
      public AssetBundle Bundle { get; }
      public bool SkipAllPostfixes { get; }
      public AsyncAssetLoadingResolve ResolveType { get; }
      public UnityEngine.Object[] Assets { get; }
   }
}
