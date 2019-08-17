namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the original parameters of the load call.
   /// </summary>
   public class AssetBundleLoadParameters
   {
      internal AssetBundleLoadParameters( string path, uint crc, ulong offset, AssetBundleLoadType loadType )
      {
         Path = path;
         Crc = crc;
         Offset = offset;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets the loaded path. Only relevant for 'LoadFromFile'.
      /// </summary>
      public string Path { get; }

      /// <summary>
      /// Gets the crc. Only relevant for 'LoadFromFile'.
      /// </summary>
      public uint Crc { get; }

      /// <summary>
      /// Gets the offset. Only relevant for 'LoadFromFile'.
      /// </summary>
      public ulong Offset { get; }

      /// <summary>
      /// Gets the type of call that is loading this asset bundle.
      /// </summary>
      public AssetBundleLoadType LoadType { get; }
   }
}
