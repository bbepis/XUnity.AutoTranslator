using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TypeExtensions
   {
      public static Type UnwrapNullable( this Type type )
      {
         return Nullable.GetUnderlyingType( type ) ?? type;
      }
   }
}
