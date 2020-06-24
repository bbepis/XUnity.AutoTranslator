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
            if( type == UnityTypes.TextField )
            {
               manipulator = new FairyGUITextComponentManipulator();
            }
            else
            {
               manipulator = new DefaultTextComponentManipulator( type );
            }
            _manipulators[ type ] = manipulator;
         }

         return manipulator;
      }

      public static bool ShouldTranslateTextComponent( this object ui, bool ignoreComponentState )
      {
         var component = ui as Component;
         if( component != null )
         {
            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return false;
            }

            if( !ignoreComponentState )
            {
               var behaviour = component as Behaviour;
               if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
               {
                  return false;
               }
            }

            var inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.InputField );
            if( inputField != null )
            {
               if( UnityTypes.InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.InputField_Properties.Placeholder.Get( inputField );
                  return ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField );
            if( inputField != null )
            {
               if( UnityTypes.TMP_InputField_Properties.Placeholder != null )
               {
                  var placeholder = UnityTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
                  return ReferenceEquals( placeholder, ui );
               }
            }

            inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput );

            return inputField == null;
         }

         return true;
      }

      public static bool IsComponentActive( this object ui )
      {
         if( ui is Component component )
         {
            return component.gameObject?.activeSelf ?? false;
         }
         return true;
      }

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableFairyGUI && UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) );
      }

      public static bool IsKnownTextMeshProType( Type type )
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.IsAssignableFrom( type );
         }
         else
         {
            return UnityTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true
            || UnityTypes.TextMeshPro?.IsAssignableFrom( type ) == true;
         }
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( UnityTypes.Text != null && UnityTypes.Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( UnityTypes.TextMesh != null && UnityTypes.TextMesh.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, type )
            || ( UnityTypes.UguiNovelText != null && UnityTypes.UguiNovelText.IsAssignableFrom( type ) )
            || ( UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom( type ) );
      }

      public static bool DoesTextMeshProSupportRichText( object ui, Type type )
      {
         if( UnityTypes.TMP_Text != null )
         {
            return UnityTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true );
         }
         else
         {
            return ( UnityTypes.TextMeshPro?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
               || ( UnityTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) );
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

         return UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom( type );
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

         if( ui is GUIContent )
         {
            ( (GUIContent)ui ).text = text;
         }
         else
         {
            // fallback to reflective approach
            GetTextManipulator( ui )?.SetText( ui, text );
         }
      }
   }
}
