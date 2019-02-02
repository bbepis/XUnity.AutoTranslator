using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class EnumerableExtensions
   {
      public static HashSet<T> ToHashSet<T>( this IEnumerable<T> ts )
      {
         var hashSet = new HashSet<T>();
         foreach( var t in ts )
         {
            hashSet.Add( t );
         }
         return hashSet;
      }
   }
}
