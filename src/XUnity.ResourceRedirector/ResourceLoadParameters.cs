using System;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the original parameters of the load call.
   /// </summary>
   public class ResourceLoadParameters
   {
      internal ResourceLoadParameters( string path, Type type, ResourceLoadType loadType )
      {
         Path = path;
         Type = type;
         LoadType = loadType;
      }

      /// <summary>
      /// Gets the name of the resource being loaded. Will not be the complete resource path if 'LoadByType' is used.
      /// </summary>
      public string Path { get; set; }

      /// <summary>
      /// Gets the type that passed to the resource load call.
      /// </summary>
      public Type Type { get; set; }

      /// <summary>
      /// Gets the type of call that loaded this resource. NOTE: 'LoadByType' usually loads a number of assets
      /// at a time. But in the context of the ResourceLoaded hook, these will be hooked as individual calls.
      /// </summary>
      public ResourceLoadType LoadType { get; }
   }
}
