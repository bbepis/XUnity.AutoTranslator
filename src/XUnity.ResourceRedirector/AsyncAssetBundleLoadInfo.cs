using UnityEngine;

namespace XUnity.ResourceRedirector
{
   internal class AsyncAssetBundleLoadInfo
   {
      public AsyncAssetBundleLoadInfo( AssetBundle bundle, string path, AsyncAssetBundleLoadingResolve resolveType )
      {
         Bundle = bundle;
         Path = path;
         ResolveType = resolveType;
      }

      public AssetBundle Bundle { get; }

      public string Path { get; }

      public AsyncAssetBundleLoadingResolve ResolveType { get; }
   }
}
