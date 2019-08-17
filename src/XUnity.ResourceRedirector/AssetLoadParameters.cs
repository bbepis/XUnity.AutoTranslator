using System;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the original parameters of the load call.
   /// </summary>
   public class AssetLoadParameters
   {
      internal AssetLoadParameters( string name, Type type, AssetLoadType loadType )
      {
         Name = name;
         Type = type;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets the name of the asset being loaded. Will be null if loaded through 'LoadMainAsset' or 'LoadByType'.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Gets the type that passed to the asset load call.
      /// </summary>
      public Type Type { get; set; }

      /// <summary>
      /// Gets the type of call that loaded this asset. NOTE: 'LoadByType' usually loads a number of assets
      /// at a time. But in the context of the AssetLoaded hook, these will be hooked as individual calls.
      /// </summary>
      public AssetLoadType LoadType { get; }
   }
}
