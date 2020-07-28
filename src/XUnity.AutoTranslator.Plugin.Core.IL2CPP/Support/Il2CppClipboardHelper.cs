using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerRuntimeLib;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   public class Il2CppClipboardHelper : IClipboardHelper
   {
      public void CopyToClipboard( string text )
      {
         try
         {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject( Il2CppType.Of<TextEditor>(), GUIUtility.keyboardControl );
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
         return false;
      }
   }
}
