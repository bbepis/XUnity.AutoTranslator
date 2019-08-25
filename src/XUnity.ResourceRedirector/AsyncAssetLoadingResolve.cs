namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum indicating how an asset load operation should be resolved.
   /// </summary>
   public enum AsyncAssetLoadingResolve
   {
      /// <summary>
      /// Indicates it should happen asynchronously through the Request property.
      /// </summary>
      ThroughRequest = 0,

      /// <summary>
      /// Indicates it should happen synchronously through the Asset/Assets property.
      /// </summary>
      ThroughAssets = 1
   }
}
