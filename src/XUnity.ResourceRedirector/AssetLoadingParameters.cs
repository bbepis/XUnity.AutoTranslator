using System;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the parameters of the load call.
   /// </summary>
   public class AssetLoadingParameters
   {
      internal AssetLoadingParameters( string name, Type type, AssetLoadType loadType )
      {
         Name = name;
         Type = type;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets or sets the name of the asset being loaded. Will be null if loaded through 'LoadMainAsset' or 'LoadByType'.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Gets or sets the type that passed to the asset load call. 
      /// </summary>
      public Type Type { get; set; }

      /// <summary>
      /// Gets or sets the type of call that loaded this asset. If 'LoadByType' or 'LoadNamedWithSubAssets' is specified
      /// multiple assets may be returned if subscribed as 'OneCallbackPerLoadCall'.
      /// </summary>
      public AssetLoadType LoadType { get; }
   }
}
