using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnhollowerRuntimeLib;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Support;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   public class Il2CppClipboardHelper : IClipboardHelper
   {
      private static readonly Func<Il2CppSystem.Type, int, object> GUIUtility_GetStateObject;

      static Il2CppClipboardHelper()
      {
         try
         {
            GUIUtility_GetStateObject =
               (Func<Il2CppSystem.Type, int, object>)ExpressionHelper.CreateTypedFastInvoke(
                  typeof( GUIUtility ).GetMethod(
                     "GetStateObject",
                     BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
                     null,
                     new Type[] { typeof( Il2CppSystem.Type ), typeof( int ) },
                     null
                  ) );
         }
         catch { }
      }

      public void CopyToClipboard( string text )
      {
         try
         {
            TextEditor editor = (TextEditor)GUIUtility_GetStateObject( Il2CppType.Of<TextEditor>(), GUIUtility.keyboardControl );
            editor.text = text;
            editor.SelectAll();
            editor.Copy();
         }
         catch( Exception e )
         {
            XuaLogger.Common.Error( e, "An error while copying text to clipboard." );
         }
      }

      public bool SupportsClipboard()
      {
         return GUIUtility_GetStateObject != null;
      }
   }
}
