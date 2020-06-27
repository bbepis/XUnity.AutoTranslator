using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
   internal static class TextTranslationInfoExtensions
   {
      public static bool GetIsKnownTextComponent( this TextTranslationInfo info )
      {
         return info != null && info.IsKnownTextComponent;
      }

      public static bool GetSupportsStabilization( this TextTranslationInfo info )
      {
         return info != null && info.SupportsStabilization;
      }
   }

   internal class TextTranslationInfo
   {
      private Action<object> _unresizeFont;
      private Action<object> _unresize;

      private int? _alteredFontSize;
      private float? _alteredLineSpacing;
      private bool _initialized = false;

      public string OriginalText { get; set; }
      public string TranslatedText { get; set; }
      public bool IsTranslated { get; set; }
      public bool IsCurrentlySettingText { get; set; } // TODO: REMOVE; Why is this even here?

      public bool IsStabilizingText { get; set; }
      public bool IsKnownTextComponent { get; set; }
      public bool SupportsStabilization { get; set; }

      public void Initialize( ITextComponent ui )
      {
         if( !_initialized )
         {
            _initialized = true;

            IsKnownTextComponent = true;
            SupportsStabilization = ui.SupportsStabilization();
         }
      }

      public static float GetComponentWidth( Component component )
      {
         // this is in it's own function because if "Text" does not exist, RectTransform likely wont exist either
         return ( (RectTransform)component.transform ).rect.width;
      }

      public void ResizeUI( object ui, UIResizeCache cache )
      {

      }

      public void UnresizeUI( object graphic )
      {
         if( graphic == null ) return;

         _unresize?.Invoke( graphic );
         _unresize = null;

         _unresizeFont?.Invoke( graphic );
         _unresizeFont = null;

         _alteredFontSize = null;
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
   }
}
