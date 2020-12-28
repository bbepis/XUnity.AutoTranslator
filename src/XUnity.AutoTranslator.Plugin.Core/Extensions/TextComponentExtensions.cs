using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      private static readonly string SupportRichTextPropertyName = "supportRichText";
      private static readonly string RichTextPropertyName = "richText";

      private static readonly Dictionary<Type, ITextComponentManipulator> _manipulators = new Dictionary<Type, ITextComponentManipulator>();

      private static ITextComponentManipulator GetTextManipulator( object ui )
      {
         var type = ui.GetType();
         if( type == null )
         {
            return null;
         }

         if( !_manipulators.TryGetValue( type, out var manipulator ) )
         {
            if( type == ClrTypes.TextField )
            {
               manipulator = new FairyGUITextComponentManipulator();
            }
            else if( type == ClrTypes.TextArea2D )
            {
               manipulator = new TextArea2DComponentManipulator();
            }
            else
            {
               manipulator = new DefaultTextComponentManipulator( type );
            }
            _manipulators[ type ] = manipulator;
         }

         return manipulator;
      }

      public static bool IsComponentActive( this object ui )
      {
         if( ui is Component component )
         {
            var go = component.gameObject;
            if( go )
            {
               if( component is Behaviour be )
               {
                  return go.activeInHierarchy && be.enabled;
               }
               else
               {
                  return go.activeInHierarchy;
               }
            }

         }
         return true;
      }

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableUGUI && ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && ClrTypes.TextMesh != null && ClrTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableFairyGUI && ClrTypes.TextField != null && ClrTypes.TextField.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) );
      }

      public static bool IsKnownTextMeshProType( Type type )
      {
         if( ClrTypes.TMP_Text != null )
         {
            return ClrTypes.TMP_Text.IsAssignableFrom( type );
         }
         else
         {
            return ClrTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true
            || ClrTypes.TextMeshPro?.IsAssignableFrom( type ) == true;
         }
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( ClrTypes.TextMesh != null && ClrTypes.TextMesh.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, type )
            || ( ClrTypes.UguiNovelText != null && ClrTypes.UguiNovelText.IsAssignableFrom( type ) )
            || ( ClrTypes.TextField != null && ClrTypes.TextField.IsAssignableFrom( type ) );
      }

      public static bool DoesTextMeshProSupportRichText( object ui, Type type )
      {
         if( ClrTypes.TMP_Text != null )
         {
            return ClrTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true );
         }
         else
         {
            return ( ClrTypes.TextMeshPro?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
               || ( ClrTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) );
         }
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         return !( ui is GUIContent );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         return ui == null || ui is GUIContent;
      }

      public static bool SupportsLineParser( this object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && ui is Component comp && Settings.GameLogTextPaths.Contains( comp.gameObject.GetPath() );
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

         TextGetterCompatModeHelper.IsGettingText = true;
         try
         {
            if( ui is GUIContent )
            {
               text = ( (GUIContent)ui ).text;
            }
            else
            {
               // fallback to reflective approach
               return GetTextManipulator( ui )?.GetText( ui );
            }
         }
         finally
         {
            TextGetterCompatModeHelper.IsGettingText = false;
         }

         return text ?? string.Empty;
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         if( ui is GUIContent gui )
         {
            gui.text = text;
         }
         else
         {
            // fallback to reflective approach
            GetTextManipulator( ui )?.SetText( ui, text );
         }
      }
   }
}
