using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ListExtensions
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="items"></param>
      /// <param name="item"></param>
      public static void BinarySearchInsert<T>( this List<T> items, T item )
         where T : IComparable<T>
      {
         var index = items.BinarySearch( item );
         if( index < 0 )
         {
            items.Insert( ~index, item );
         }
         else
         {
            items.Insert( index, item );
         }
      }
   }
}
