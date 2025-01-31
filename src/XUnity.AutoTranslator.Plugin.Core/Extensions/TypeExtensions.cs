using System;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TypeExtensions
   {
      public static Type UnwrapNullable( this Type type )
      {
         return Nullable.GetUnderlyingType( type ) ?? type;
      }

      public static bool IsAssemblyCsharp( this Assembly originalAssembly)
      {
         return originalAssembly.GetName().Name.Equals( "Assembly-CSharp" );
      }
   }
}
