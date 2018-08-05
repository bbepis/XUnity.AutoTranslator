using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class ComponentExtensions
   {
      private static readonly string TextPropertyName = "text";

      public static string GetText( this object ui )
      {
         string text = null;
         var type = ui.GetType();

         if( type == Constants.Types.UguiNovelText && ( (Component)ui ).gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.AdvUguiSelection ) != null )
         {
            // these texts are handled by AdvCommand, unless it is a selection
            text = ( (Text)ui ).text;
         }
         else if( ui is Text )
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
         var type = ui.GetType();

         if( type == Constants.Types.UguiNovelText && ( ( Component ) ui ).gameObject.GetFirstComponentInSelfOrAncestor( Constants.Types.AdvUguiSelection ) != null )
         {
            // these texts are handled by AdvCommand, unless it is a selection
            ( (Text)ui ).text = text;
         }
         else if( ui is Text )
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
