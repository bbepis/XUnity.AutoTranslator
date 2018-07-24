﻿using System;
using System.Linq;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Constants
{
   public static class Types
   {
      public static readonly Type TMP_InputField = FindType( "TMPro.TMP_InputField" );
      public static readonly Type TMP_Text = FindType( "TMPro.TMP_Text" );
      public static readonly Type TextMeshProUGUI = FindType( "TMPro.TextMeshProUGUI" );
      public static readonly Type TextMeshPro = FindType( "TMPro.TextMeshPro" );

      public static readonly Type InputField = FindType( "UnityEngine.UI.InputField" );
      public static readonly Type Text = FindType( "UnityEngine.UI.Text" );

      public static readonly Type GUI = FindType( "UnityEngine.GUI" );

      public static readonly Type UILabel = FindType( "UILabel" );

      public static readonly Type WWW = FindType( "UnityEngine.WWW" );

      private static Type FindType( string name )
      {
         return AppDomain.CurrentDomain.GetAssemblies()
            .Select( x => x.GetType( name, false ) )
            .Where( x => x != null )
            .FirstOrDefault();
      }
   }
}
