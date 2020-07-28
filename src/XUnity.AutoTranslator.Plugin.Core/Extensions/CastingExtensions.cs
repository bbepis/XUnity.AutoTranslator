using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.Common.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class CastingExtensions
   {
      private static readonly ITypeCastingHelper Helper = TypeCastingHelper.Instance;

      public static bool TryCastTo<TObject>( this object obj, out TObject castedObject )
      {
         return Helper.TryCastTo( obj, out castedObject );
      }
   }
}
