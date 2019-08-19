using System;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Enum indicating how the resource redirector should treat the callback.
   /// </summary>
   public enum HookBehaviour
   {
      /// <summary>
      /// Specifies that exactly one callback should be received per call to asset/resource load method.
      /// </summary>
      OneCallbackPerLoadCall = 1,

      /// <summary>
      /// Specifies that exactly one callback should be received per loaded asset/resources. This means
      /// that the 'Asset' property should be used over the 'Assets' property on the context object.
      /// Do note that when using this option, if no resources are returned by a load call, no callbacks
      /// will be received.
      /// </summary>
      OneCallbackPerResourceLoaded = 2
   }
}
