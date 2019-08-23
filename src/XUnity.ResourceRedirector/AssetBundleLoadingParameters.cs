namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the parameters of the load call.
   /// </summary>
   public class AssetBundleLoadingParameters
   {
      internal AssetBundleLoadingParameters( string path, uint crc, ulong offset, AssetBundleLoadType loadType )
      {
         Path = path;
         Crc = crc;
         Offset = offset;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets or sets the loaded path. Only relevant for 'LoadFromFile'.
      /// </summary>
      public string Path { get; set; }

      /// <summary>
      /// Gets or sets the crc. Only relevant for 'LoadFromFile'.
      /// </summary>
      public uint Crc { get; set; }

      /// <summary>
      /// Gets or sets the offset. Only relevant for 'LoadFromFile'.
      /// </summary>
      public ulong Offset { get; set; }

      /// <summary>
      /// Gets the type of call that is loading this asset bundle.
      /// </summary>
      public AssetBundleLoadType LoadType { get; }
   }
}
