using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class AsyncAssetBundleLoadInfo
   {
      public AsyncAssetBundleLoadInfo( AssetBundleLoadingParameters parameters, AssetBundle bundle, bool skipAllPostfixes, AsyncAssetBundleLoadingResolve resolveType )
      {
         Parameters = parameters;
         Bundle = bundle;
         SkipAllPostfixes = skipAllPostfixes;
         ResolveType = resolveType;
      }

      public AssetBundleLoadingParameters Parameters { get; }

      public AssetBundle Bundle { get; }

      public bool SkipAllPostfixes { get; }

      public AsyncAssetBundleLoadingResolve ResolveType { get; }
   }
}
