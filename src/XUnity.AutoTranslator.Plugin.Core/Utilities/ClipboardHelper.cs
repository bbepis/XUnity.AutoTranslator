using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class ClipboardHelper
   {
      public static void CopyToClipboard( IEnumerable<string> lines, int maxCharacters )
      {
         var texts = lines.ToList();

         var builder = new StringBuilder();
         for( int i = 0; i < texts.Count; i++ )
         {
            var line = texts[ i ];
            if( line.Length + builder.Length > maxCharacters ) break;

            if( i == texts.Count - 1 )
            {
               builder.Append( line );
            }
            else
            {
               builder.AppendLine( line );
            }
         }

         var text = builder.ToString();

         CopyToClipboard( text );
      }

      public static void CopyToClipboard( string text )
      {
         try
         {
#if MANAGED
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );
#else
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject( Il2CppType.Of<TextEditor>(), GUIUtility.keyboardControl );
#endif
            editor.text = text;
            editor.SelectAll();
            editor.Copy();
         }
         catch( Exception e )
         {
            XuaLogger.Common.Error( e, "An error while copying text to clipboard." );
         }
      }
   }
}
