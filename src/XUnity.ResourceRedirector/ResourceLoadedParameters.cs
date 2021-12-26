using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the parameters of the load call.
   /// </summary>
   public class ResourceLoadedParameters
   {
#if MANAGED
      internal ResourceLoadedParameters( string path, Type type, ResourceLoadType loadType )
#else
      internal ResourceLoadedParameters( string path, Il2CppSystem.Type type, ResourceLoadType loadType )
#endif
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
#if MANAGED
      public Type Type { get; set; }
#else
      public Il2CppSystem.Type Type { get; set; }
#endif

      /// <summary>
      /// Gets the type of call that loaded this asset. If 'LoadByType' is specified
      /// multiple assets may be returned if subscribed as 'OneCallbackPerLoadCall'.
      /// </summary>
      public ResourceLoadType LoadType { get; }
   }
}
