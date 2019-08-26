namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum indicating how an asset bundle load operation should be resolved.
   /// </summary>
   public enum AsyncAssetBundleLoadingResolve
   {
      /// <summary>
      /// Indicates it should happen asynchronously through the Request property.
      /// </summary>
      ThroughRequest = 0,

      /// <summary>
      /// Indicates it should happen synchronously through the Bundle property.
      /// </summary>
      ThroughBundle = 1
   }
}
