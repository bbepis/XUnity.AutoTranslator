using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class AsyncAssetLoadInfo
   {
      public AsyncAssetLoadInfo(
         AssetLoadingParameters parameters,
         AssetBundle bundle,
         bool skipAllPostfixes,
         AsyncAssetLoadingResolve resolveType,
         UnityEngine.Object[] assets )
      {
         Parameters = parameters;
         Bundle = bundle;
         SkipAllPostfixes = skipAllPostfixes;
         ResolveType = resolveType;
         Assets = assets;
      }

      public AssetLoadingParameters Parameters { get; }
      public AssetBundle Bundle { get; }
      public bool SkipAllPostfixes { get; }
      public AsyncAssetLoadingResolve ResolveType { get; }
      public UnityEngine.Object[] Assets { get; }
   }
}
