using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Fonts;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.RuntimeHooker.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{

   internal class TextTranslationInfo
   {
      private static readonly string MultiLinePropertyName = "multiLine";
      private static readonly string OverflowMethodPropertyName = "overflowMethod";
      private static readonly string OverflowModePropertyName = "overflowMode";

      private Action<object> _unresize;
      private Action<object> _unfont;

      private bool _hasCheckedTypeWriter;
      private MonoBehaviour _typewriter;
      private object _alteredSpacing;
      private int _translationFrame = -1;

      public TextTranslationInfo()
      {
      }

      public string OriginalText { get; set; }

      public string TranslatedText { get; set; }

      public bool IsTranslated { get; set; }

      public bool IsCurrentlySettingText { get; set; }

      public void ResetScrollIn( object graphic )
      {
         if( !_hasCheckedTypeWriter )
         {
            _hasCheckedTypeWriter = true;

            if( ClrTypes.Typewriter != null )
            {
               var ui = graphic as Component;
               if( ui != null )
               {
                  _typewriter = (MonoBehaviour)ui.GetComponent( ClrTypes.Typewriter );
               }
            }
         }

         if( _typewriter != null )
         {
            AccessToolsShim.Method( ClrTypes.Typewriter, "OnEnable" )?.Invoke( _typewriter, null );
         }
      }

      public void ChangeFont( object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) )
         {
            if( string.IsNullOrEmpty( Settings.OverrideFont ) ) return;

            var Text_fontProperty = ClrTypes.Text.CachedProperty( "font" );
            var Text_fontSizeProperty = ClrTypes.Text.CachedProperty( "fontSize" );

            var previousFont = (Font)Text_fontProperty.Get( ui );
            var Font_fontSizeProperty = previousFont.GetType().CachedProperty( "fontSize" );

            var newFont = FontCache.GetOrCreate( (int?)Font_fontSizeProperty?.Get( previousFont ) ?? (int)Text_fontSizeProperty.Get( ui ) );
            if( newFont == null || previousFont == null ) return;

            if( !ReferenceEquals( newFont, previousFont ) )
            {
               Text_fontProperty.Set( ui, newFont );
               _unfont = obj =>
               {
                  Text_fontProperty.Set( obj, previousFont );
               };
            }
         }
         else if( ( ClrTypes.TextMeshPro != null && ClrTypes.TextMeshPro.IsAssignableFrom( type ) )
            || ( ClrTypes.TextMeshProUGUI != null && ClrTypes.TextMeshProUGUI.IsAssignableFrom( type ) ) )
         {
            if( string.IsNullOrEmpty( Settings.OverrideFontTextMeshPro ) ) return;

            var fontProperty = ReflectionCache.CachedProperty( type, "font" );

            var previousFont = fontProperty.Get( ui );
            var newFont = FontCache.GetOrCreateTextMeshProFont();
            if( newFont == null || previousFont == null ) return;

            if( !ReferenceEquals( newFont, previousFont ) )
            {
               fontProperty.Set( ui, newFont );
               _unfont = obj =>
               {
                  fontProperty.Set( obj, previousFont );
               };
            }
         }
      }

      public void UnchangeFont( object ui )
      {
         if( ui == null ) return;

         _unfont?.Invoke( ui );
         _unfont = null;
      }

      public static float GetComponentWidth( Component component )
      {
         // this is in it's own function because if "Text" does not exist, RectTransform likely wont exist either
         return ( (RectTransform)component.transform ).rect.width;
      }

      public void ResizeUI( object ui )
      {
         // do not resize if there is no object of ir it is already resized
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) )
         {
            var text = (Component)ui;

            // text is likely to be longer than there is space for, simply expand out anyway then
            var componentWidth = GetComponentWidth( text );
            var quarterScreenSize = Screen.width / 4;
            var isComponentWide = componentWidth > quarterScreenSize;

            var horizontalOverflowProperty = ClrTypes.Text.CachedProperty( "horizontalOverflow" );
            var verticalOverflowProperty = ClrTypes.Text.CachedProperty( "verticalOverflow" );
            var lineSpacingProperty = ClrTypes.Text.CachedProperty( "lineSpacing" );
            var resizeTextForBestFitProperty = ClrTypes.Text.CachedProperty( "resizeTextForBestFit" );

            // width < quarterScreenSize is used to determine the likelihood of a text using multiple lines
            // the idea is, if the UI element is larger than the width of half the screen, there is a larger
            // likelihood that it will go into multiple lines too.
            var originalHorizontalOverflow = horizontalOverflowProperty.Get( text );
            var originalVerticalOverflow = verticalOverflowProperty.Get( text );
            var originalLineSpacing = lineSpacingProperty.Get( text );

            if( isComponentWide && !(bool)resizeTextForBestFitProperty.Get( text ) )
            {
               horizontalOverflowProperty.Set( text, 0 /* HorizontalWrapMode.Wrap */ );
               verticalOverflowProperty.Set( text, 1 /* VerticalWrapMode.Overflow */ );
               if( Settings.ResizeUILineSpacingScale.HasValue && !Equals( _alteredSpacing, originalLineSpacing ) )
               {
                  var alteredSpacing = (float)originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
                  _alteredSpacing = alteredSpacing;
                  lineSpacingProperty.Set( text, alteredSpacing );
               }

               if( _unresize == null )
               {
                  _unresize = g =>
                  {
                     horizontalOverflowProperty.Set( g, originalHorizontalOverflow );
                     verticalOverflowProperty.Set( g, originalVerticalOverflow );
                     if( Settings.ResizeUILineSpacingScale.HasValue )
                     {
                        lineSpacingProperty.Set( g, originalLineSpacing );
                     }
                  };
               }
            }
         }
         else
         {
            // special handling for NGUI to better handle textbox sizing
            if( type == ClrTypes.UILabel )
            {
               var originalMultiLine = type.GetProperty( MultiLinePropertyName )?.GetGetMethod()?.Invoke( ui, null );
               var originalOverflowMethod = type.GetProperty( OverflowMethodPropertyName )?.GetGetMethod()?.Invoke( ui, null );
               //var originalSpacingY = graphic.GetSpacingY();

               type.GetProperty( MultiLinePropertyName )?.GetSetMethod()?.Invoke( ui, new object[] { true } );
               type.GetProperty( OverflowMethodPropertyName )?.GetSetMethod()?.Invoke( ui, new object[] { 0 } );
               //if( Settings.ResizeUILineSpacingScale.HasValue && !Equals( _alteredSpacing, originalSpacingY ) )
               //{
               //   var alteredSpacing = originalSpacingY.Multiply( Settings.ResizeUILineSpacingScale.Value );
               //   _alteredSpacing = alteredSpacing;
               //   graphic.SetSpacingY( alteredSpacing );
               //}

               if( _unresize == null )
               {
                  _unresize = g =>
                  {
                     var gtype = g.GetType();
                     gtype.GetProperty( MultiLinePropertyName )?.GetSetMethod()?.Invoke( g, new object[] { originalMultiLine } );
                     gtype.GetProperty( OverflowMethodPropertyName )?.GetSetMethod()?.Invoke( g, new object[] { originalOverflowMethod } );
                     //if( Settings.ResizeUILineSpacingScale.HasValue )
                     //{
                     //   g.SetSpacingY( originalSpacingY );
                     //}
                  };
               }
            }
            else if( type == ClrTypes.TextMeshPro || type == ClrTypes.TextMeshProUGUI )
            {
               var originalOverflowMode = ClrTypes.TMP_Text?.GetProperty( OverflowModePropertyName )?.GetValue( ui, null );

               // ellipsis (1) works
               // masking (2) has a tendency to break in some versions of TMP
               // truncate (3) works
               if( originalOverflowMode != null && (int)originalOverflowMode == 2 )
               {
                  ClrTypes.TMP_Text.GetProperty( OverflowModePropertyName ).SetValue( ui, 3, null );

                  _unresize = g =>
                  {
                     ClrTypes.TMP_Text.GetProperty( OverflowModePropertyName ).SetValue( g, 2, null );
                  };
               }

            }
         }
      }

      public void UnresizeUI( object graphic )
      {
         if( graphic == null ) return;

         _unresize?.Invoke( graphic );
         _unresize = null;
      }

      public void Reset( string newText )
      {
         if( Settings.RequiresToggleFix )
         {
            var frame = Time.frameCount;
            if( frame != _translationFrame )
            {
               IsTranslated = false;
               TranslatedText = null;
               OriginalText = newText;
            }
         }
         else
         {
            IsTranslated = false;
            TranslatedText = null;
            OriginalText = newText;
         }
      }

      public void SetTranslatedText( string translatedText )
      {
         if( Settings.RequiresToggleFix )
         {
            _translationFrame = Time.frameCount;

            IsTranslated = true;
            TranslatedText = translatedText;
         }
         else
         {
            IsTranslated = true;
            TranslatedText = translatedText;
         }
      }
   }
}
