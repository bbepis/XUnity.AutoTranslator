using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class ExceptionExtensions
   {
      public static TException FirstInnerExceptionOfType<TException>( this Exception e )
         where TException : Exception
      {
         var current = e;
         while( current != null )
         {
            if( current is TException ) return (TException)current;
            current = current.InnerException;
         }

         return null;
      }
   }
}
