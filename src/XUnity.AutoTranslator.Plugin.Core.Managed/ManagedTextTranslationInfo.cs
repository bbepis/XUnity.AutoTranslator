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
using XUnity.AutoTranslator.Plugin.Shims;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ManagedTextTranslationInfo : TextTranslationInfo
   {
      private Action<object> _unresizeFont;
      private Action<object> _unresize;
      private Action<object> _unfont;

      private bool _hasCheckedTypeWriter;
      private MonoBehaviour _typewriter;

      private int? _alteredFontSize;
      private float? _alteredLineSpacing;

      public override bool ShouldIgnoreTextComponent( object ui )
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

            var inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.InputField );
            if( inputField != null )
            {
               if( UnityTypes.InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.InputField_Properties.Placeholder.Get( inputField );
                  return !ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField );
            if( inputField != null )
            {
               if( UnityTypes.TMP_InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
                  return !ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput );

            return inputField != null;
         }

         return false;
      }

      public override void ResetScrollIn( object graphic )
      {
         if( !_hasCheckedTypeWriter )
         {
            _hasCheckedTypeWriter = true;

            if( UnityTypes.Typewriter != null )
            {
               var ui = graphic as Component;
               if( ui != null )
               {
                  _typewriter = (MonoBehaviour)ui.GetComponent( UnityTypes.Typewriter );
               }
            }
         }

         if( _typewriter != null )
         {
            AccessToolsShim.Method( UnityTypes.Typewriter, "OnEnable" )?.Invoke( _typewriter, null );
         }
      }

      public override void ChangeFont( object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
         {
            if( string.IsNullOrEmpty( Settings.OverrideFont ) ) return;

            var Text_fontProperty = UnityTypes.Text_Properties.Font;
            var Text_fontSizeProperty = UnityTypes.Text_Properties.FontSize;

            var previousFont = (Font)Text_fontProperty.Get( ui );
            var Font_fontSizeProperty = UnityTypes.Font_Properties.FontSize;

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
         else if( ( UnityTypes.TextMeshPro != null && UnityTypes.TextMeshPro.IsAssignableFrom( type ) )
            || ( UnityTypes.TextMeshProUGUI != null && UnityTypes.TextMeshProUGUI.IsAssignableFrom( type ) ) )
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

      public override void UnchangeFont( object ui )
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

      public override void ResizeUI( object ui, UIResizeCache cache )
      {
         // do not resize if there is no object of ir it is already resized
         if( ui == null ) return;

         var type = ui.GetType();

         if( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
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
               var scope = TranslationScopeHelper.GetScope( ui );
               if( cache.TryGetUIResize( segments, scope, out var result ) )
               {
                  if( result.AutoResizeCommand != null )
                  {
                     var resizeTextForBestFitValue = UnityTypes.Text_Properties.ResizeTextForBestFit.Get( ui );

                     if( resizeTextForBestFitValue != null )
                     {
                        var resizeTextMinSizeValue = UnityTypes.Text_Properties.ResizeTextMinSize?.Get( ui );
                        var resizeTextMaxSizeValue = UnityTypes.Text_Properties.ResizeTextMaxSize?.Get( ui );

                        var minSize = result.AutoResizeCommand.GetMinSize() ?? 1d;
                        if( UnityTypes.Text_Properties.ResizeTextMinSize != null )
                        {
                           int minSizeCorrected = double.IsNaN( minSize ) ? 1 : (int)minSize;
                           UnityTypes.Text_Properties.ResizeTextMinSize.Set( ui, minSizeCorrected );
                        }

                        var maxSize = result.AutoResizeCommand.GetMaxSize();
                        if( maxSize.HasValue && UnityTypes.Text_Properties.ResizeTextMaxSize != null )
                        {
                           int maxSizeCorrected = double.IsNaN( maxSize.Value ) ? 1 : (int)maxSize.Value; // 1 == infinitely large
                           UnityTypes.Text_Properties.ResizeTextMaxSize?.Set( ui, maxSizeCorrected );
                        }

                        var shouldAutoResize = result.AutoResizeCommand.ShouldAutoResize();
                        UnityTypes.Text_Properties.ResizeTextForBestFit.Set( ui, shouldAutoResize );

                        if( isUntouched )
                        {
                           _unresizeFont += g =>
                           {
                              UnityTypes.Text_Properties.ResizeTextForBestFit.Set( g, resizeTextForBestFitValue );
                           };

                           if( UnityTypes.Text_Properties.ResizeTextMinSize != null )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.ResizeTextMinSize.Set( g, resizeTextMinSizeValue );
                              };
                           }

                           if( maxSize.HasValue && UnityTypes.Text_Properties.ResizeTextMaxSize != null )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.ResizeTextMaxSize.Set( g, resizeTextMaxSizeValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.ResizeCommand != null )
                  {
                     var currentFontSize = (int?)UnityTypes.Text_Properties.FontSize.Get( ui );

                     if( currentFontSize.HasValue && !Equals( _alteredFontSize, currentFontSize ) )
                     {
                        var newFontSize = result.ResizeCommand.GetSize( currentFontSize.Value );
                        if( newFontSize.HasValue )
                        {
                           UnityTypes.Text_Properties.FontSize.Set( ui, newFontSize.Value );
                           _alteredFontSize = newFontSize.Value;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.FontSize.Set( g, currentFontSize );
                              };
                           }
                        }
                     }
                  }

                  if( result.LineSpacingCommand != null )
                  {
                     var lineSpacingValue = (float?)UnityTypes.Text_Properties.LineSpacing.Get( ui );

                     if( lineSpacingValue.HasValue && !Equals( _alteredLineSpacing, lineSpacingValue ) )
                     {
                        var newLineSpacingValue = result.LineSpacingCommand.GetLineSpacing( lineSpacingValue.Value );
                        if( newLineSpacingValue.HasValue )
                        {
                           isLineSpacingSet = true;
                           UnityTypes.Text_Properties.LineSpacing.Set( ui, newLineSpacingValue.Value );
                           _alteredLineSpacing = newLineSpacingValue;

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.LineSpacing.Set( g, lineSpacingValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.HorizontalOverflowCommand != null )
                  {
                     var horizontalOverflowValue = UnityTypes.Text_Properties.HorizontalOverflow.Get( ui );

                     if( horizontalOverflowValue != null )
                     {
                        var newHorizontalOverflowValue = result.HorizontalOverflowCommand.GetMode();
                        if( newHorizontalOverflowValue.HasValue )
                        {
                           isHorizontalOverflowSet = true;
                           UnityTypes.Text_Properties.HorizontalOverflow.Set( ui, newHorizontalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.HorizontalOverflow.Set( g, horizontalOverflowValue );
                              };
                           }
                        }
                     }
                  }

                  if( result.VerticalOverflowCommand != null )
                  {
                     var verticalOverflowValue = UnityTypes.Text_Properties.VerticalOverflow.Get( ui );

                     if( verticalOverflowValue != null )
                     {
                        var newVerticalOverflowValue = result.VerticalOverflowCommand.GetMode();
                        if( newVerticalOverflowValue.HasValue )
                        {
                           isVerticalOverflowSet = true;
                           UnityTypes.Text_Properties.VerticalOverflow.Set( ui, newVerticalOverflowValue.Value );

                           if( isUntouched )
                           {
                              _unresizeFont += g =>
                              {
                                 UnityTypes.Text_Properties.VerticalOverflow.Set( g, verticalOverflowValue );
                              };
                           }
                        }
                     }
                  }
               }
            }

            if( isComponentWide && ( UnityTypes.Text_Properties.ResizeTextForBestFit == null || !(bool)UnityTypes.Text_Properties.ResizeTextForBestFit.Get( text ) ) )
            {
               if( !isLineSpacingSet && Settings.ResizeUILineSpacingScale.HasValue && UnityTypes.Text_Properties.LineSpacing != null )
               {
                  var originalLineSpacing = UnityTypes.Text_Properties.LineSpacing.Get( text );

                  if( !Equals( _alteredLineSpacing, originalLineSpacing ) )
                  {
                     var newLineSpacing = (float)originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
                     UnityTypes.Text_Properties.LineSpacing.Set( text, newLineSpacing );
                     _alteredLineSpacing = newLineSpacing;

                     if( isUntouched )
                     {
                        _unresizeFont += g =>
                        {
                           UnityTypes.Text_Properties.LineSpacing.Set( g, originalLineSpacing );
                        };
                     }
                  }
               }

               if( !isVerticalOverflowSet && UnityTypes.Text_Properties.VerticalOverflow != null )
               {
                  var originalVerticalOverflow = UnityTypes.Text_Properties.VerticalOverflow.Get( text );
                  UnityTypes.Text_Properties.VerticalOverflow.Set( text, 1 /* VerticalWrapMode.Overflow */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        UnityTypes.Text_Properties.VerticalOverflow.Set( g, originalVerticalOverflow );
                     };
                  }
               }

               if( !isHorizontalOverflowSet && UnityTypes.Text_Properties.HorizontalOverflow != null )
               {
                  var originalHorizontalOverflow = UnityTypes.Text_Properties.HorizontalOverflow.Get( text );
                  UnityTypes.Text_Properties.HorizontalOverflow.Set( text, 0 /* HorizontalWrapMode.Wrap */ );

                  if( isUntouched )
                  {
                     _unresizeFont += g =>
                     {
                        UnityTypes.Text_Properties.HorizontalOverflow.Set( g, originalHorizontalOverflow );
                     };
                  }
               }
            }
         }
         else if( type == UnityTypes.UILabel )
         {
            // special handling for NGUI to better handle textbox sizing
            
            var useFloatSpacingPropertyValue = UnityTypes.UILabel_Properties.UseFloatSpacing?.Get( ui );
            var spacingXPropertyValue = UnityTypes.UILabel_Properties.SpacingX?.Get( ui );
            var multiLinePropertyValue = UnityTypes.UILabel_Properties.MultiLine?.Get( ui );
            var overflowMethodPropertyValue = UnityTypes.UILabel_Properties.OverflowMethod?.Get( ui );

            UnityTypes.UILabel_Properties.UseFloatSpacing?.Set( ui, false );
            UnityTypes.UILabel_Properties.SpacingX?.Set( ui, -1 );
            UnityTypes.UILabel_Properties.MultiLine?.Set( ui, true );
            UnityTypes.UILabel_Properties.OverflowMethod?.Set( ui, 0 );

            if( _unresize == null )
            {
               _unresize = g =>
               {
                  UnityTypes.UILabel_Properties.UseFloatSpacing?.Set( g, useFloatSpacingPropertyValue );
                  UnityTypes.UILabel_Properties.SpacingX?.Set( g, spacingXPropertyValue );
                  UnityTypes.UILabel_Properties.MultiLine?.Set( g, multiLinePropertyValue );
                  UnityTypes.UILabel_Properties.OverflowMethod?.Set( g, overflowMethodPropertyValue );
               };
            }
         }
         else if( type == UnityTypes.TextMeshPro || type == UnityTypes.TextMeshProUGUI )
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
               var scope = TranslationScopeHelper.GetScope( ui );
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

      public override void UnresizeUI( object graphic )
      {
         if( graphic == null ) return;

         _unresize?.Invoke( graphic );
         _unresize = null;

         _unresizeFont?.Invoke( graphic );
         _unresizeFont = null;

         _alteredFontSize = null;
      }
   }
}
