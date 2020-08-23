using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.UIResize;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal abstract class TextTranslationInfo
   {
      private bool _initialized = false;

      public string OriginalText { get; set; }
      public string TranslatedText { get; set; }
      public bool IsTranslated { get; set; }
      public bool IsCurrentlySettingText { get; set; }

      public bool IsStabilizingText { get; set; }
      public bool IsKnownTextComponent { get; set; }
      public bool SupportsStabilization { get; set; }
      public bool ShouldIgnore { get; set; }

      public IReadOnlyTextTranslationCache TextCache { get; set; }

      public void Initialize( object ui )
      {
         if( !_initialized )
         {
            _initialized = true;

            IsKnownTextComponent = ui.IsKnownTextType();
            SupportsStabilization = ui.SupportsStabilization();
            ShouldIgnore = ShouldIgnoreTextComponent( ui );
         }
      }

      public void Reset( string newText )
      {
         IsTranslated = false;
         TranslatedText = null;
         OriginalText = newText;
      }

      public void SetTranslatedText( string translatedText )
      {
         IsTranslated = true;
         TranslatedText = translatedText;
      }

      public abstract bool ShouldIgnoreTextComponent( object ui );

      public abstract void ResetScrollIn( object ui );

      public abstract void ChangeFont( object ui );

      public abstract void UnchangeFont( object ui );

      public abstract void ResizeUI( object ui, UIResizeCache cache );

      public abstract void UnresizeUI( object ui );
   }
}
