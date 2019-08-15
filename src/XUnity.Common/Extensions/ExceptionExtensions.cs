using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Extensions
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ExceptionExtensions
   {

      /// <summary>
      /// WARNING: Pubternal API (internal)v. Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="TException"></typeparam>
      /// <param name="e"></param>
      /// <returns></returns>
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
