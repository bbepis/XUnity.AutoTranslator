using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class ExceptionExtensions
   {
      public static bool IsCausedBy<TException>( this Exception e )
      {
         var current = e;
         while( current != null )
         {
            if( e is TException ) return true;

            current = current.InnerException;
         }

         return false;
      }
   }
}
