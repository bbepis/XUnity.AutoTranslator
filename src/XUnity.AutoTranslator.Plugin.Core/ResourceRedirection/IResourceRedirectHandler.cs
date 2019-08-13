using System;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   /// <summary>
   /// This interface can be registered to handle resource redirection events.
   /// </summary>
   internal interface IResourceRedirectHandler
   {

   }

   /// <summary>
   /// This interface can be registered to handle resource redirection events.
   /// </summary>
   /// <typeparam name="TAsset">This is the type of asset that is subscribed to.</typeparam>
   internal interface IResourceRedirectHandler<TAsset> : IResourceRedirectHandler
      where TAsset : UnityEngine.Object
   {
      /// <summary>
      /// Method to be invoked whenever a resource of the specified type is redirected.
      /// </summary>
      /// <param name="context">A context containing all relevant information of the resource redirection.</param>
      void Handle( IRedirectionContext<TAsset> context );
   }
}
