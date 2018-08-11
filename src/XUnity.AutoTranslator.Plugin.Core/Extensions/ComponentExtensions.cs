﻿using System;
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
