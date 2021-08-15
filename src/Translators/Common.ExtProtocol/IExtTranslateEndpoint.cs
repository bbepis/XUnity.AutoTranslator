using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtProtocol
{
   /// <summary>
   /// The interface that must be implemented by a translator.
   /// </summary>
   public interface IExtTranslateEndpoint
   {
      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine",
      /// so it can be implemented in an asynchronous fashion.
      /// </summary>
      Task Translate( ITranslationContext context );

      /// <summary>
      /// Initializes the endpoint with the specified configuration.
      /// </summary>
      /// <param name="config"></param>
      void Initialize( string config );
   }
}
