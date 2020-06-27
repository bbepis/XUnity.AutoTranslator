using Il2CppSystem.Runtime.Remoting.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      public static bool ShouldTranslateTextComponent( this ITextComponent ui, bool ignoreComponentState )
      {
         return true;

         //var component = ui.Component;
         //if( component != null )
         //{
         //   // dummy check
         //   var go = component.gameObject;
         //   var ignore = go.HasIgnoredName();
         //   if( ignore )
         //   {
         //      return false;
         //   }

         //   if( !ignoreComponentState )
         //   {
         //      var behaviour = component.TryCast<Behaviour>();
         //      if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
         //      {
         //         return false;
         //      }
         //   }

         //   var inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.InputField );
         //   if( inputField != null )
         //   {
         //      if( UnityTypes.InputField_Properties.Placeholder != null )
         //      {
         //         var placeholder = UnityTypes.InputField_Properties.Placeholder.Get( inputField );
         //         return ReferenceEquals( placeholder, ui );
         //      }
         //   }

         //   inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField );
         //   if( inputField != null )
         //   {
         //      if( UnityTypes.TMP_InputField_Properties.Placeholder != null )
         //      {
         //         var placeholder = UnityTypes.TMP_InputField_Properties.Placeholder.Get( inputField );
         //         return ReferenceEquals( placeholder, ui );
         //      }
         //   }

         //   inputField = go.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput );

         //   return inputField == null;
         //}

         //return true;
      }

      public static bool IsComponentActive( this ITextComponent ui )
      {
         return ui.GameObject?.activeSelf ?? false;
      }

      public static ITextComponent AsTextComponent( this Component ui )
      {
         if( ui == null ) return null;

         var type = ui.GetIl2CppType();

         if( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.Il2CppType.IsAssignableFrom( type ) )
         {
            return new TextComponent( ui );
         }
         else if( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.Il2CppType.IsAssignableFrom( type ) )
         {
            return new TextMeshComponent( ui );
         }
         else if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.Il2CppType.IsAssignableFrom( type ) )
         {
            return new TMP_TextComponent( ui );
         }

         return null;
      }

      public static bool SupportsStabilization( this ITextComponent ui )
      {
         if( ui == null ) return false;

         return true;
      }

      public static bool SupportsLineParser( this ITextComponent ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && Settings.GameLogTextPaths.Contains( ui.Component.gameObject.GetPath() );
      }

      public static string GetText( this ITextComponent ui )
      {
         if( ui == null ) return null;

         return ui.text ?? string.Empty;
      }

      public static void SetText( this ITextComponent ui, string text )
      {
         if( ui == null ) return;

         ui.text = text;
      }
   }
}
