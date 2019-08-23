using System;
using UnityEngine;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Class representing the parameters of the load call.
   /// </summary>
   public class ResourceLoadedParameters
   {
      internal ResourceLoadedParameters( string path, Type type, ResourceLoadType loadType )
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
      /// Gets the type of call that loaded this asset. If 'LoadByType' is specified
      /// multiple assets may be returned if subscribed as 'OneCallbackPerLoadCall'.
      /// </summary>
      public ResourceLoadType LoadType { get; }
   }
}
