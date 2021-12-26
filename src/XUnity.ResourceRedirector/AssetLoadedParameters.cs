using System;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the parameters of the load call.
   /// </summary>
   public class AssetLoadedParameters
   {
#if MANAGED
      internal AssetLoadedParameters( string name, Type type, AssetLoadType loadType )
#else
      internal AssetLoadedParameters( string name, Il2CppSystem.Type type, AssetLoadType loadType )
#endif
      {
         Name = name;
         Type = type;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets the name of the asset being loaded. Will be null if loaded through 'LoadMainAsset' or 'LoadByType'.
      /// </summary>
      public string Name { get; }

      /// <summary>
      /// Gets the type that passed to the asset load call. 
      /// </summary>
#if MANAGED
      public Type Type { get; }
#else
      public Il2CppSystem.Type Type { get; }
#endif

      /// <summary>
      /// Gets the type of call that loaded this asset. If 'LoadByType' or 'LoadNamedWithSubAssets' is specified
      /// multiple assets may be returned if subscribed as 'OneCallbackPerLoadCall'.
      /// </summary>
      public AssetLoadType LoadType { get; }
   }
}
