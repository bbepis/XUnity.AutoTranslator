using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Fonts;

namespace XUnity.AutoTranslator.Plugin.Core
{

   internal class TextTranslationInfo
   {
      private static readonly string MultiLinePropertyName = "multiLine";
      private static readonly string OverflowMethodPropertyName = "overflowMethod";

      private Action<object> _unresize;
      private Action<object> _unfont;

      private bool _hasCheckedTypeWriter;
      private MonoBehaviour _typewriter;
      private object _alteredSpacing;

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

            if( Constants.ClrTypes.Typewriter != null )
            {
               var ui = graphic as Component;
               if( ui != null )
               {
                  _typewriter = (MonoBehaviour)ui.GetComponent( Constants.ClrTypes.Typewriter );
               }
            }
         }

         if( _typewriter != null )
         {
            AccessTools.Method( Constants.ClrTypes.Typewriter, "OnEnable" )?.Invoke( _typewriter, null );
         }
      }

      public void ChangeFont( object graphic )
      {
         if( graphic == null ) return;

         if( graphic is Text )
         {
            var ui = graphic as Text;

            var previousFont = ui.font;
            var newFont = FontCache.GetOrCreate( previousFont?.fontSize ?? ui.fontSize );

            if( !ReferenceEquals( newFont, previousFont ) )
            {
               ui.font = newFont;
               _unfont = obj =>
               {
                  ( (Text)obj ).font = previousFont;
               };
            }
         }
      }

      public void UnchangeFont( object graphic )
      {
         if( graphic == null ) return;

         _unfont?.Invoke( graphic );
         _unfont = null;
      }

      public void ResizeUI( object graphic )
      {
         // do not resize if there is no object of ir it is already resized
         if( graphic == null ) return;

         if( graphic is Text )
         {
            var ui = (Text)graphic;
            
            // text is likely to be longer than there is space for, simply expand out anyway then
            var componentWidth = ( (RectTransform)ui.transform ).rect.width;
            var quarterScreenSize = Screen.width / 4;
            var isComponentWide = componentWidth > quarterScreenSize;

            // width < quarterScreenSize is used to determine the likelihood of a text using multiple lines
            // the idea is, if the UI element is larger than the width of half the screen, there is a larger
            // likelihood that it will go into multiple lines too.
            var originalHorizontalOverflow = ui.horizontalOverflow;
            var originalVerticalOverflow = ui.verticalOverflow;
            var originalLineSpacing = ui.lineSpacing;

            if( isComponentWide && !ui.resizeTextForBestFit )
            {
               ui.horizontalOverflow = HorizontalWrapMode.Wrap;
               ui.verticalOverflow = VerticalWrapMode.Overflow;
               if( Settings.ResizeUILineSpacingScale.HasValue && !Equals( _alteredSpacing, originalLineSpacing ) )
               {
                  var alteredSpacing = originalLineSpacing * Settings.ResizeUILineSpacingScale.Value;
                  _alteredSpacing = alteredSpacing;
                  ui.lineSpacing = alteredSpacing;
               }

               if( _unresize == null )
               {
                  _unresize = g =>
                  {
                     var gui = (Text)g;
                     gui.horizontalOverflow = originalHorizontalOverflow;
                     gui.verticalOverflow = originalVerticalOverflow;
                     if( Settings.ResizeUILineSpacingScale.HasValue )
                     {
                        gui.lineSpacing = originalLineSpacing;
                     }
                  };
               }
            }
         }
         else
         {
            var type = graphic.GetType();

            // special handling for NGUI to better handle textbox sizing
            if( type == Constants.ClrTypes.UILabel )
            {
               var originalMultiLine = type.GetProperty( MultiLinePropertyName )?.GetGetMethod()?.Invoke( graphic, null );
               var originalOverflowMethod = type.GetProperty( OverflowMethodPropertyName )?.GetGetMethod()?.Invoke( graphic, null );
               //var originalSpacingY = graphic.GetSpacingY();

               type.GetProperty( MultiLinePropertyName )?.GetSetMethod()?.Invoke( graphic, new object[] { true } );
               type.GetProperty( OverflowMethodPropertyName )?.GetSetMethod()?.Invoke( graphic, new object[] { 0 } );
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
         }
      }

      public void UnresizeUI( object graphic )
      {
         if( graphic == null ) return;

         _unresize?.Invoke( graphic );
         _unresize = null;
      }

      public TextTranslationInfo Reset( string newText )
      {
         IsTranslated = false;
         TranslatedText = null;
         OriginalText = newText;

         return this;
      }

      public void SetTranslatedText( string translatedText )
      {
         IsTranslated = true;
         TranslatedText = translatedText;
      }
   }
}
