using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class Features
   {
      public static readonly bool SupportsClipboard = false;

      public static readonly bool SupportsCustomYieldInstruction = false;

      static Features()
      {
         try
         {
            SupportsClipboard = ClrTypes.TextEditor?.GetProperty( "text" )?.GetSetMethod() != null;
         }
         catch( Exception )
         {
            
         }

         try
         {
            SupportsCustomYieldInstruction = ClrTypes.CustomYieldInstruction != null;
         }
         catch( Exception )
         {
            
         }
      }
   }
}
