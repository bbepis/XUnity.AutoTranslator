using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   /// <summary>
   /// Class that wraps useful methods on WWW, so the developer
   /// of a plugin do not need to take a direct reference to
   /// UnityEngine.dll
   /// </summary>
   public static class WwwHelper
   {
      /// <summary>
      /// Escapes the supplied url.
      /// </summary>
      /// <param name="url"></param>
      /// <returns></returns>
      public static string EscapeUrl( string url )
      {
         return WWW.EscapeURL( url );
      }

      /// <summary>
      /// Unescapes the supplied url.
      /// </summary>
      /// <param name="url"></param>
      /// <returns></returns>
      public static string UnescapeUrl( string url )
      {
         return WWW.UnEscapeURL( url );
      }
   }
}
