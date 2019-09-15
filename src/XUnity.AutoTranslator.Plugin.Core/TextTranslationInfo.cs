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
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.AutoTranslator.Plugins.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{

   internal class TextTranslationInfo
   {
      private static readonly string MultiLinePropertyName = "multiLine";
      private static readonly string OverflowMethodPropertyName = "overflowMethod";
      private static readonly string OverflowModePropertyName = "overflowMode";
      private static readonly string SpacingXPropertyName = "spacingX";
      private static readonly string UseFloatSpacingPropertyName = "useFloatSpacing";

      private Action<object> _unresizeFont;
      private Action<object> _unresize;
      private Action<object> _unfont;

      private bool _hasCheckedTypeWriter;
      private MonoBehaviour _typewriter;
      private int _translationFrame = -1;

      private int? _alteredFontSize;
      private float? _alteredLineSpacing;

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

      public void ResizeUI( object ui, UIResizeCache cache )
      {
         // do not resize if there is no object of ir it is already resized
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) )
         {
            var text = (Component)ui;

            // text is likely to be longer than there is space for, simply expand out anyway then

            // width < quarterScreenSize is used to determine the likelihood of a text using multiple lines
            // the idea is, if the UI element is larger than the width of half the screen, there is a larger
            // likelihood that it will go into multiple lines too.
            var componentWidth = GetComponentWidth( text );
            var quarterScreenSize = Screen.width / 4;
            var isComponentWide = componentWidth > quarterScreenSize;
            var horizontalOverflowProperty = ClrTypes.Text.CachedProperty( "horizontalOverflow" );
            var verticalOverflowProperty = ClrTypes.Text.CachedProperty( "verticalOverflow" );
            var lineSpacingProperty = ClrTypes.Text.CachedProperty( "lineSpacing" );
            var resizeTextForBestFitProperty = ClrTypes.Text.CachedProperty( "resizeTextForBestFit" );


            bool isLineSpacingSet = false;
            bool isHorizontalOverflowSet = false;
            bool isVerticalOverflowSet = false;
            bool isUntouched = _unresizeFont == null;
            if( cache.HasAnyResizeCommands )
            {
               var segments = text.gameObject.GetPathSegments(); // TODO: Perhaps... cache these segments?????
               var scope = TranslationScopeProvider.GetScope( ui );
               if( cache.TryGetUIResize( segments, scope, out var result ) )
               {
                  if( result.AutoResizeCommand != null )
                  {
                     var resizeTextForBestFitValue = resizeTextForBestFitProperty.Get( ui );

                     if( resizeTextForBestFitValue != null )
                     {
                        var resizeTextMinSizeProperty = type.CachedProperty( "resizeTextMinSize" );
                        var resizeTextMinSizeValue = resizeTextMinSizeProperty?.Get( ui );

                        var shouldAutoResize = result.AutoResizeCommand.ShouldAutoResize();
                        resizeTextForBestFitProperty.Set( ui, shouldAutoResize );

                        if( resizeTextMinSizeProperty != null )
                        {
                           resizeTextMinSizeProperty.Set( ui, 1 );
                        }

                        if( isUntouched )
                        {
                           _unresizeFont += g =>
                           {
                              resizeTextForBestFitProperty.Set( g, resizeTextForBestFitValue );

                              if( resizeTextMinSizeProperty != null )
                              {
                                 resizeTextMinSizeProperty.Set( ui, resizeTextMinSizeValue );
                              }
                           };
                        }
                     }
                  }

                  if( result.ResizeCommand != null )
                  {
                     var fontSizeProperty = ClrTypes.Text.CachedProperty( "fontSize" );
                     var currentFontSize = (int?)fontSizeProperty.Get( ui );

                     if( currentFontSize.HasValue && !Equals( _alteredFontSize, currentFontSize ) )
                     {
                        var newFontSize = result.ResizeCommand.GetSize( currentFontSize.Value );
                        if( newFontSize.HasValue )
                        {
                           fontSizeProperty.Set( ui, newFontSize.Value );
                           _alteredFontSize = newFontSize.Value;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 fontSizeProperty.Set( g, currentFontSize );
                              };
                           }
                        }
                     }
                  }

                  if( result.LineSpacingCommand != null )
                  {
                     var lineSpacingValue = (float?)lineSpacingProperty.Get( ui );

                     if( lineSpacingValue.HasValue && !Equals( _alteredLineSpacing, lineSpacingValue ) )
                     {
                        var newLineSpacingValue = result.LineSpacingCommand.GetLineSpacing( lineSpacingValue.Value );
                        if( newLineSpacingValue.HasValue )
                        {
                           isLineSpacingSet = true;
                           lineSpacingProperty.Set( ui, newLineSpacingValue.Value );
                           _alteredLineSpacing = newLineSpacingValue;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 lineSpacingProperty.Set( g, lineSpacingValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.HorizontalOverflowCommand != null )
                  {
                     var horizontalOverflowValue = horizontalOverflowProperty.Get( ui );

                     if( horizontalOverflowValue != null )
                     {
                        var newHorizontalOverflowValue = result.HorizontalOverflowCommand.GetMode();
                        if( newHorizontalOverflowValue.HasValue )
                        {
                           isHorizontalOverflowSet = true;
                           horizontalOverflowProperty.Set( ui, newHorizontalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 horizontalOverflowProperty.Set( g, horizontalOverflowValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.VerticalOverflowCommand != null )
                  {
                     var verticalOverflowValue = verticalOverflowProperty.Get( ui );

                     if( verticalOverflowValue != null )
                     {
                        var newVerticalOverflowValue = result.VerticalOverflowCommand.GetMode();
                        if( newVerticalOverflowValue.HasValue )
                        {
                           isVerticalOverflowSet = true;
                           verticalOverflowProperty.Set( ui, newVerticalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 verticalOverflowProperty.Set( g, verticalOverflowValue );
                              };
                           }
                        }
                     }
                  }
               }
            }

            bool isBestFit = (bool)resizeTextForBestFitProperty.Get( text );
            if( isComponentWide && !isBestFit )
            {
               if( !isLineSpacingSet && Settings.ResizeUILineSpacingScale.HasValue )
               {
                  var originalLineSpacing = lineSpacingProperty.Get( text );

                  if( !Equals( _alteredLineSpacing, originalLineSpacing ) )
                  {
                     var newLineSpacing = (float)originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
                     lineSpacingProperty.Set( text, newLineSpacing );
                     _alteredLineSpacing = newLineSpacing;

                     if( isUntouched )
                     {
                        _unresizeFont += g =>
                        {
                           lineSpacingProperty.Set( g, originalLineSpacing );
                        };
                     }
                  }
               }

               if( !isVerticalOverflowSet )
               {
                  var originalVerticalOverflow = verticalOverflowProperty.Get( text );
                  verticalOverflowProperty.Set( text, 1 /* VerticalWrapMode.Overflow */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        verticalOverflowProperty.Set( g, originalVerticalOverflow );
                     };
                  }
               }

               if( !isHorizontalOverflowSet )
               {
                  var originalHorizontalOverflow = horizontalOverflowProperty.Get( text );
                  horizontalOverflowProperty.Set( text, 0 /* HorizontalWrapMode.Wrap */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        horizontalOverflowProperty.Set( g, originalHorizontalOverflow );
                     };
                  }
               }
            }
         }
         else if( type == ClrTypes.UILabel )
         {
            // special handling for NGUI to better handle textbox sizing

            var useFloatSpacingProperty = type.CachedProperty( UseFloatSpacingPropertyName );
            var spacingXProperty = type.CachedProperty( SpacingXPropertyName );
            var multiLineProperty = type.CachedProperty( MultiLinePropertyName );
            var overflowMethodProperty = type.CachedProperty( OverflowMethodPropertyName );

            var useFloatSpacingPropertyValue = useFloatSpacingProperty?.Get( ui );
            var spacingXPropertyValue = spacingXProperty?.Get( ui );
            var multiLinePropertyValue = multiLineProperty?.Get( ui );
            var overflowMethodPropertyValue = overflowMethodProperty?.Get( ui );

            useFloatSpacingProperty?.Set( ui, false );
            spacingXProperty?.Set( ui, -1 );
            multiLineProperty?.Set( ui, true );
            overflowMethodProperty?.Set( ui, 0 );

            if( _unresize == null )
            {
               _unresize = g =>
               {
                  useFloatSpacingProperty?.Set( g, useFloatSpacingPropertyValue );
                  spacingXProperty?.Set( g, spacingXPropertyValue );
                  multiLineProperty?.Set( g, multiLinePropertyValue );
                  overflowMethodProperty?.Set( g, overflowMethodPropertyValue );
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

            if( cache.HasAnyResizeCommands )
            {
               bool isUntouched = _unresizeFont == null;

               var text = (Component)ui;
               var t = text.GetType();

               var segments = text.gameObject.GetPathSegments();
               var scope = TranslationScopeProvider.GetScope( ui );
               if( cache.TryGetUIResize( segments, scope, out var result ) )
               {
                  if( result.AutoResizeCommand != null )
                  {
                     var enableAutoSizingProperty = type.CachedProperty( "enableAutoSizing" );
                     var enableAutoSizingValue = enableAutoSizingProperty.Get( ui );

                     if( enableAutoSizingValue != null )
                     {
                        var shouldAutoResize = result.AutoResizeCommand.ShouldAutoResize();
                        enableAutoSizingProperty.Set( ui, shouldAutoResize );

                        if( isUntouched )
                        {
                           _unresizeFont += g =>
                           {
                              enableAutoSizingProperty.Set( g, enableAutoSizingValue );
                           };
                        }
                     }
                  }

                  if( result.ResizeCommand != null )
                  {
                     var fontSizeProperty = t.CachedProperty( "fontSize" );
                     var currentFontSize = (float?)fontSizeProperty.Get( ui );

                     if( currentFontSize.HasValue )
                     {
                        var currentFontSizeInt = (int)currentFontSize.Value;
                        if( !Equals( _alteredFontSize, currentFontSizeInt ) )
                        {
                           var newFontSize = result.ResizeCommand.GetSize( (int)currentFontSize.Value );
                           if( newFontSize.HasValue )
                           {
                              fontSizeProperty.Set( ui, (float)newFontSize.Value );
                              _alteredFontSize = newFontSize.Value;

                              if( isUntouched )
                              {
                                 _unresizeFont += g =>
                                 {
                                    fontSizeProperty.Set( g, currentFontSize );
                                 };
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
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
