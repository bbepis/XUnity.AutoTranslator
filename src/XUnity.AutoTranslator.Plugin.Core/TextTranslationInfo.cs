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
      private Action<object> _unfont;
      private HashSet<string> _redirectedTranslations;

      private bool _hasCheckedTypeWriter;
      private MonoBehaviour _typewriter;
      private int _translationFrame = -1;

      private int? _alteredFontSize;
      private float? _alteredLineSpacing;
      private bool _initialized = false;

      public string OriginalText { get; set; }
      public string TranslatedText { get; set; }
      public bool IsTranslated { get; set; }
      public bool IsCurrentlySettingText { get; set; } // TODO: REMOVE; Why is this even here?
      public bool ShouldIgnore { get; set; }

      public bool IsStabilizingText { get; set; }
      public bool IsKnownTextComponent { get; set; }
      public bool SupportsStabilization { get; set; }

      public HashSet<string> RedirectedTranslations => _redirectedTranslations ?? ( _redirectedTranslations = new HashSet<string>() );

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

      public bool ShouldIgnoreTextComponent( object ui )
      {
         if( ui is Component component )
         {
            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return true;
            }

            var inputField = go.GetFirstComponentInSelfOrAncestor( ClrTypes.InputField );
            if( inputField != null )
            {
               if( ClrTypes.InputField_Properties.Placeholder != null )
               {
                  var placeholder = ClrTypes.InputField_Properties.Placeholder.Get( inputField );
                  return !ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( ClrTypes.TMP_InputField );
            if( inputField != null )
            {
               if( ClrTypes.TMP_InputField_Properties.Placeholder != null )
               {
                  var placeholder = ClrTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
                  return !ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( ClrTypes.UIInput );

            return inputField != null;
         }

         return false;
      }

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

         if( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) && ( ClrTypes.Font_Properties.FontSize != null || Settings.OverrideFontSize.HasValue ) )
         {
            if( string.IsNullOrEmpty( Settings.OverrideFont ) ) return;

            var Text_fontProperty = ClrTypes.Text_Properties.Font;

            var previousFont = (Font)Text_fontProperty.Get( ui );

            var newFont = FontCache.GetOrCreate( Settings.OverrideFontSize ?? (int)ClrTypes.Font_Properties.FontSize.Get( previousFont ) );
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

            var fontProperty = type.CachedProperty( "font" );

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
                     var resizeTextForBestFitValue = ClrTypes.Text_Properties.ResizeTextForBestFit.Get( ui );

                     if( resizeTextForBestFitValue != null )
                     {
                        var resizeTextMinSizeValue = ClrTypes.Text_Properties.ResizeTextMinSize?.Get( ui );
                        var resizeTextMaxSizeValue = ClrTypes.Text_Properties.ResizeTextMaxSize?.Get( ui );

                        var minSize = result.AutoResizeCommand.GetMinSize() ?? 1d;
                        if( ClrTypes.Text_Properties.ResizeTextMinSize != null )
                        {
                           int minSizeCorrected = double.IsNaN( minSize ) ? 1 : (int)minSize;
                           ClrTypes.Text_Properties.ResizeTextMinSize.Set( ui, minSizeCorrected );
                        }

                        var maxSize = result.AutoResizeCommand.GetMaxSize();
                        if( maxSize.HasValue && ClrTypes.Text_Properties.ResizeTextMaxSize != null )
                        {
                           int maxSizeCorrected = double.IsNaN( maxSize.Value ) ? 1 : (int)maxSize.Value; // 1 == infinitely large
                           ClrTypes.Text_Properties.ResizeTextMaxSize?.Set( ui, maxSizeCorrected );
                        }

                        var shouldAutoResize = result.AutoResizeCommand.ShouldAutoResize();
                        ClrTypes.Text_Properties.ResizeTextForBestFit.Set( ui, shouldAutoResize );

                        if( isUntouched )
                        {
                           _unresizeFont += g =>
                           {
                              ClrTypes.Text_Properties.ResizeTextForBestFit.Set( g, resizeTextForBestFitValue );
                           };

                           if( ClrTypes.Text_Properties.ResizeTextMinSize != null )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.ResizeTextMinSize.Set( g, resizeTextMinSizeValue );
                              };
                           }

                           if( maxSize.HasValue && ClrTypes.Text_Properties.ResizeTextMaxSize != null )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.ResizeTextMaxSize.Set( g, resizeTextMaxSizeValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.ResizeCommand != null )
                  {
                     var currentFontSize = (int?)ClrTypes.Text_Properties.FontSize.Get( ui );

                     if( currentFontSize.HasValue && !Equals( _alteredFontSize, currentFontSize ) )
                     {
                        var newFontSize = result.ResizeCommand.GetSize( currentFontSize.Value );
                        if( newFontSize.HasValue )
                        {
                           ClrTypes.Text_Properties.FontSize.Set( ui, newFontSize.Value );
                           _alteredFontSize = newFontSize.Value;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.FontSize.Set( g, currentFontSize );
                              };
                           }
                        }
                     }
                  }

                  if( result.LineSpacingCommand != null )
                  {
                     var lineSpacingValue = (float?)ClrTypes.Text_Properties.LineSpacing.Get( ui );

                     if( lineSpacingValue.HasValue && !Equals( _alteredLineSpacing, lineSpacingValue ) )
                     {
                        var newLineSpacingValue = result.LineSpacingCommand.GetLineSpacing( lineSpacingValue.Value );
                        if( newLineSpacingValue.HasValue )
                        {
                           isLineSpacingSet = true;
                           ClrTypes.Text_Properties.LineSpacing.Set( ui, newLineSpacingValue.Value );
                           _alteredLineSpacing = newLineSpacingValue;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.LineSpacing.Set( g, lineSpacingValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.HorizontalOverflowCommand != null )
                  {
                     var horizontalOverflowValue = ClrTypes.Text_Properties.HorizontalOverflow.Get( ui );

                     if( horizontalOverflowValue != null )
                     {
                        var newHorizontalOverflowValue = result.HorizontalOverflowCommand.GetMode();
                        if( newHorizontalOverflowValue.HasValue )
                        {
                           isHorizontalOverflowSet = true;
                           ClrTypes.Text_Properties.HorizontalOverflow.Set( ui, newHorizontalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.HorizontalOverflow.Set( g, horizontalOverflowValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.VerticalOverflowCommand != null )
                  {
                     var verticalOverflowValue = ClrTypes.Text_Properties.VerticalOverflow.Get( ui );

                     if( verticalOverflowValue != null )
                     {
                        var newVerticalOverflowValue = result.VerticalOverflowCommand.GetMode();
                        if( newVerticalOverflowValue.HasValue )
                        {
                           isVerticalOverflowSet = true;
                           ClrTypes.Text_Properties.VerticalOverflow.Set( ui, newVerticalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 ClrTypes.Text_Properties.VerticalOverflow.Set( g, verticalOverflowValue );
                              };
                           }
                        }
                     }
                  }
               }
            }

            if( isComponentWide && ( ClrTypes.Text_Properties.ResizeTextForBestFit == null || !(bool)ClrTypes.Text_Properties.ResizeTextForBestFit.Get( text ) ) )
            {
               if( !isLineSpacingSet && Settings.ResizeUILineSpacingScale.HasValue && ClrTypes.Text_Properties.LineSpacing != null )
               {
                  var originalLineSpacing = ClrTypes.Text_Properties.LineSpacing.Get( text );

                  if( !Equals( _alteredLineSpacing, originalLineSpacing ) )
                  {
                     var newLineSpacing = (float)originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
                     ClrTypes.Text_Properties.LineSpacing.Set( text, newLineSpacing );
                     _alteredLineSpacing = newLineSpacing;

                     if( isUntouched )
                     {
                        _unresizeFont += g =>
                        {
                           ClrTypes.Text_Properties.LineSpacing.Set( g, originalLineSpacing );
                        };
                     }
                  }
               }

               if( !isVerticalOverflowSet && ClrTypes.Text_Properties.VerticalOverflow != null )
               {
                  var originalVerticalOverflow = ClrTypes.Text_Properties.VerticalOverflow.Get( text );
                  ClrTypes.Text_Properties.VerticalOverflow.Set( text, 1 /* VerticalWrapMode.Overflow */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        ClrTypes.Text_Properties.VerticalOverflow.Set( g, originalVerticalOverflow );
                     };
                  }
               }

               if( !isHorizontalOverflowSet && ClrTypes.Text_Properties.HorizontalOverflow != null )
               {
                  var originalHorizontalOverflow = ClrTypes.Text_Properties.HorizontalOverflow.Get( text );
                  ClrTypes.Text_Properties.HorizontalOverflow.Set( text, 0 /* HorizontalWrapMode.Wrap */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        ClrTypes.Text_Properties.HorizontalOverflow.Set( g, originalHorizontalOverflow );
                     };
                  }
               }
            }
         }
         else if( type == ClrTypes.UILabel )
         {
            // special handling for NGUI to better handle textbox sizing

            var useFloatSpacingPropertyValue = ClrTypes.UILabel_Properties.UseFloatSpacing?.Get( ui );
            var spacingXPropertyValue = ClrTypes.UILabel_Properties.SpacingX?.Get( ui );
            var multiLinePropertyValue = ClrTypes.UILabel_Properties.MultiLine?.Get( ui );
            var overflowMethodPropertyValue = ClrTypes.UILabel_Properties.OverflowMethod?.Get( ui );

            ClrTypes.UILabel_Properties.UseFloatSpacing?.Set( ui, false );
            ClrTypes.UILabel_Properties.SpacingX?.Set( ui, -1 );
            ClrTypes.UILabel_Properties.MultiLine?.Set( ui, true );
            ClrTypes.UILabel_Properties.OverflowMethod?.Set( ui, 0 );

            if( _unresize == null )
            {
               _unresize = g =>
               {
                  ClrTypes.UILabel_Properties.UseFloatSpacing?.Set( g, useFloatSpacingPropertyValue );
                  ClrTypes.UILabel_Properties.SpacingX?.Set( g, spacingXPropertyValue );
                  ClrTypes.UILabel_Properties.MultiLine?.Set( g, multiLinePropertyValue );
                  ClrTypes.UILabel_Properties.OverflowMethod?.Set( g, overflowMethodPropertyValue );
               };
            }
         }
         else if( type == ClrTypes.TextMeshPro || type == ClrTypes.TextMeshProUGUI )
         {
            var overflowModeProperty = type.CachedProperty( "overflowMode" );
            var originalOverflowMode = overflowModeProperty?.Get( ui );

            // ellipsis (1) works
            // masking (2) has a tendency to break in some versions of TMP
            // truncate (3) works
            if( originalOverflowMode != null && (int)originalOverflowMode == 2 )
            {
               overflowModeProperty.Set( ui, 3 );

               _unresize = g =>
               {
                  overflowModeProperty.Set( g, 2 );
               };
            }

            if( cache.HasAnyResizeCommands )
            {
               bool isUntouched = _unresizeFont == null;

               var text = (Component)ui;

               var segments = text.gameObject.GetPathSegments();
               var scope = TranslationScopeProvider.GetScope( ui );
               if( cache.TryGetUIResize( segments, scope, out var result ) )
               {
                  if( result.AutoResizeCommand != null )
                  {
                     var enableAutoSizingProperty = type.CachedProperty( "enableAutoSizing" );
                     var fontSizeMinProperty = type.CachedProperty( "fontSizeMin" );
                     var fontSizeMaxProperty = type.CachedProperty( "fontSizeMax" );

                     if( enableAutoSizingProperty != null )
                     {
                        var enableAutoSizingValue = enableAutoSizingProperty.Get( ui );
                        var fontSizeMinValue = fontSizeMinProperty?.Get( ui );
                        var fontSizeMaxValue = fontSizeMaxProperty?.Get( ui );

                        var minSize = result.AutoResizeCommand.GetMinSize();
                        if( minSize.HasValue && fontSizeMinProperty != null )
                        {
                           float minSizeCorrected = double.IsNaN( minSize.Value ) ? 0f : (float)minSize.Value;
                           fontSizeMinProperty?.Set( ui, minSizeCorrected );
                        }

                        var maxSize = result.AutoResizeCommand.GetMaxSize();
                        if( maxSize.HasValue && fontSizeMaxProperty != null )
                        {
                           float maxSizeCorrected = double.IsNaN( maxSize.Value ) ? float.MaxValue : (float)maxSize.Value;
                           fontSizeMaxProperty?.Set( ui, maxSizeCorrected );
                        }

                        var shouldAutoResize = result.AutoResizeCommand.ShouldAutoResize();
                        enableAutoSizingProperty.Set( ui, shouldAutoResize );

                        if( isUntouched )
                        {
                           _unresizeFont += g =>
                           {
                              enableAutoSizingProperty.Set( g, enableAutoSizingValue );
                           };

                           if( minSize.HasValue && fontSizeMinProperty != null )
                           {
                              _unresizeFont += g =>
                              {
                                 fontSizeMinProperty.Set( g, fontSizeMinValue );
                              };
                           }

                           if( maxSize.HasValue && fontSizeMaxProperty != null )
                           {
                              _unresizeFont += g =>
                              {
                                 fontSizeMaxProperty.Set( g, fontSizeMaxValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.ResizeCommand != null )
                  {
                     var fontSizeProperty = type.CachedProperty( "fontSize" );
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
