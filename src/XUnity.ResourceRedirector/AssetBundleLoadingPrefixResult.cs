namespace XUnity.ResourceRedirector
{
   internal struct AssetBundleLoadingPrefixResult
   {
      public AssetBundleLoadingPrefixResult( AssetBundleLoadingParameters parameters, bool skipOriginalCall )
      {
         Parameters = parameters;
         SkipOriginalCall = skipOriginalCall;
      }

      public AssetBundleLoadingParameters Parameters { get; }
      public bool SkipOriginalCall { get; }
   }
}
