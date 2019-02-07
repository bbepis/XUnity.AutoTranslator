using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class TextHelper
   {
      internal static string Decode( string text )
      {
         var commentIndex = text.IndexOf( "//" );
         if( commentIndex > -1 )
         {
            text = text.Substring( 0, commentIndex );
         }

         return text.Replace( "\\r", "\r" )
            .Replace( "\\n", "\n" )
            .Replace( "%3D", "=" )
            .Replace( "%2F%2F", "//" );
      }

      internal static string Encode( string text )
      {
         return text.Replace( "\r", "\\r" )
            .Replace( "\n", "\\n" )
            .Replace( "=", "%3D" )
            .Replace( "//", "%2F%2F" );
      }
   }
}
