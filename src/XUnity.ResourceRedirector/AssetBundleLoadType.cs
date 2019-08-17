namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum representing the different ways an asset bundle may be loaded.
   /// </summary>
   public enum AssetBundleLoadType
   {
      /// <summary>
      /// Indicates that the asset bundle is being loaded through a call to 'LoadFromFile' or 'LoadFromFileAsync'.
      /// </summary>
      LoadFromFile = 1,
      //LoadFromMemory,

      // other places, online resource, etc.
   }
}
