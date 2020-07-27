using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Shims;

namespace XUnity.Common.Managed.Shims
{
   public class ManagedClipboardHelper : IClipboardHelper
   {
      public void CopyToClipboard( string text )
      {
         try
         {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );
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
         return UnityFeatures.SupportsClipboard;
      }
   }
}
