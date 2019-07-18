using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class DetourExtensions
   {
      public static T GenerateTrampolineEx<T>( this object detour )
      {
         return (T)detour.GetType()
            .GetMethods()
            .Where( x => x.Name == "GenerateTrampoline" && x.IsGenericMethod )
            .FirstOrDefault()
            .MakeGenericMethod( typeof( T ) )
            .Invoke( detour, null );
      }
   }
}
