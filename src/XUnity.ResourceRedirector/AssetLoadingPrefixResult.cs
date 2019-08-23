namespace XUnity.ResourceRedirector
{
   internal struct AssetLoadingPrefixResult
   {
      public AssetLoadingPrefixResult( AssetLoadingParameters parameters, bool skipOriginalCall, bool skipAllPostfixes )
      {
         Parameters = parameters;
         SkipOriginalCall = skipOriginalCall;
         SkipAllPostfixes = skipAllPostfixes;
      }

      public AssetLoadingParameters Parameters { get; }
      public bool SkipOriginalCall { get; }
      public bool SkipAllPostfixes { get; }
   }
}
