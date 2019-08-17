namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum representing the different ways an asset may be loaded.
   /// </summary>
   public enum AssetLoadType
   {
      /// <summary>
      /// Indicates that this asset has been loaded as the 'mainAsset' in the AssetBundle API.
      /// </summary>
      LoadMainAsset = 1,

      /// <summary>
      /// Indicates that this call is loading all assets of a specific type in an AssetBundle API.
      /// </summary>
      LoadByType = 2,

      /// <summary>
      /// Indicates that this call is loading a specific named asset in the AssetBundle API.
      /// </summary>
      LoadNamed = 3,

      /// <summary>
      /// Indicates that this call is loading a specific named asset and all those below it in the AssetBundle API.
      /// </summary>
      LoadNamedWithSubAssets = 4
   }
}
