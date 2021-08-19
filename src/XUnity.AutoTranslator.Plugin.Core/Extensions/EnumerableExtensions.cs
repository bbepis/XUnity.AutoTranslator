using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class EnumerableExtensions
   {
#if MANAGED
      public static HashSet<T> ToHashSet<T>( this IEnumerable<T> ts )
      {
         var hashSet = new HashSet<T>();
         foreach( var t in ts )
         {
            hashSet.Add( t );
         }
         return hashSet;
      }

      public static HashSet<T> ToHashSet<T>( this IEnumerable<T> ts, IEqualityComparer<T> equalityComparer )
      {
         var hashSet = new HashSet<T>( equalityComparer );
         foreach( var t in ts )
         {
            hashSet.Add( t );
         }
         return hashSet;
      }
#endif

      public static void AddRange<T>( this ICollection<T> collection, IEnumerable<T> values )
      {
         foreach( var value in values )
         {
            collection.Add( value );
         }
      }
   }
}
