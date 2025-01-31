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

      private static Assembly _assemblyCsharp;
      public static bool IsAssemblyCsharp( this Assembly assembly )
      {
         if( assembly == null ) throw new ArgumentNullException( nameof( assembly ) );

         if( _assemblyCsharp != null )
            return _assemblyCsharp.Equals( assembly );

         var isAssemblyCsharp = assembly.GetName().Name.Equals( "Assembly-CSharp" );

         if( isAssemblyCsharp )
            _assemblyCsharp = assembly;

         return isAssemblyCsharp;
      }

      public static bool IsAssemblyCsharpFirstpass( this Assembly assembly )
      {
         if( assembly == null ) throw new ArgumentNullException( nameof( assembly ) );

         return assembly.GetName().Name.Equals( "Assembly-CSharp-firstpass" );
      }
   }
}
