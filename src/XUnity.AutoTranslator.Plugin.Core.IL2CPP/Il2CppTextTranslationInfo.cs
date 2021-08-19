using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.AutoTranslator.Plugin.Shims;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class Il2CppTextTranslationInfo : TextTranslationInfo
   {
      public override bool ShouldIgnoreTextComponent( object ui )
      {
         if( ui is ITextComponent tc && tc.Component != null )
         {
            var component = tc.Component;

            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return true;
            }

            return tc.IsPlaceholder();
         }

         return false;
      }

      public override void ChangeFont( object ui )
      {
      }

      public override void UnchangeFont( object ui )
      {
      }

      public override void ResizeUI( object ui, UIResizeCache cache )
      {
      }

      public override void UnresizeUI( object ui )
      {
      }

      public override void ResetScrollIn( object ui )
      {
      }
   }
}
