using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.MonoMod
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class DetourExtensions
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="detour"></param>
      /// <returns></returns>
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
