namespace XUnity.ResourceRedirector
{
   internal struct AssetOrResourceLoadingPrefixResult
   {
      public AssetOrResourceLoadingPrefixResult( bool skipOriginalCall, bool skipAllPostfixes )
      {
         SkipOriginalCall = skipOriginalCall;
         SkipAllPostfixes = skipAllPostfixes;
      }

      public bool SkipOriginalCall { get; }
      public bool SkipAllPostfixes { get; }
   }
}
