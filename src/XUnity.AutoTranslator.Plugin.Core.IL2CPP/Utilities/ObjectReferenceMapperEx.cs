using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class ObjectReferenceMapperEx
   {
      public static TextTranslationInfo GetOrCreateTextTranslationInfo( this ITextComponent ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            var info = ui.GetOrCreateExtensionData<TextTranslationInfo>();
            info.Initialize( ui );

            return info;
         }

         return null;
      }

      public static TextTranslationInfo GetTextTranslationInfo( this ITextComponent ui )
      {
         if( !ui.IsSpammingComponent() )
         {
            var info = ui.GetExtensionData<TextTranslationInfo>();

            return info;
         }

         return null;
      }
   }
}
