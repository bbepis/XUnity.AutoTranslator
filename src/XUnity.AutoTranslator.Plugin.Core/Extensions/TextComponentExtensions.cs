using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      private static readonly string RichTextPropertyName = "richText";
      private static readonly string TextPropertyName = "text";

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableUGUI && ui is Text )
            || ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableNGUI && ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) )
            || ( Settings.EnableUtage && ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) );
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui as Text )?.supportRichText == true
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.GetProperty( RichTextPropertyName )?.GetValue( ui, null ), true ) )
            || ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )
            || ( ClrTypes.UguiNovelText != null && ClrTypes.UguiNovelText.IsAssignableFrom( type ) );
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         // shortcircuit for spammy component, to avoid reflective calls
         if( ui is GUIContent ) return false;

         var type = ui.GetType();

         return ui is Text
            || ( ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) );
      }

      public static bool SupportsLineParser( this object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && ui is Component comp && Settings.GameLogTextPaths.Contains( comp.gameObject.GetPath() );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         if( ui == null ) return true;

         return ui is GUIContent;
      }

      public static bool IsWhitelistedForImmediateRichTextTranslation( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type );
      }

      public static bool IsNGUI( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type );
      }

      public static string GetText( this object ui )
      {
         if( ui == null ) return null;

         string text = null;
         var type = ui.GetType();

         if( ui is Text )
         {
            text = ( (Text)ui ).text;
         }
         else if( ui is GUIContent )
         {
            text = ( (GUIContent)ui ).text;
         }
         else
         {
            // fallback to reflective approach
            text = (string)ui.GetType()?.GetProperty( TextPropertyName )?.GetValue( ui, null );
         }

         return text ?? string.Empty;
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ui is Text )
         {
            ( (Text)ui ).text = text;
         }
         else if( ui is GUIContent )
         {
            ( (GUIContent)ui ).text = text;
         }
         else
         {
            // fallback to reflective approach
            type.GetProperty( TextPropertyName )?.GetSetMethod()?.Invoke( ui, new[] { text } );
         }
      }
   }
}
