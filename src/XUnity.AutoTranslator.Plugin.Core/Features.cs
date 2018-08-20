using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class Features
   {
      public static readonly bool SupportsClipboard = false;

      static Features()
      {
         try
         {
            SupportsClipboard = typeof( TextEditor )?.GetProperty( "text" )?.GetSetMethod() != null;
         }
         catch( Exception )
         {
            
         }
      }
   }
}
