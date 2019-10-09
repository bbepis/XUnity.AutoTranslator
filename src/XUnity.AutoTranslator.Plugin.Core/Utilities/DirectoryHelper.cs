using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class DirectoryHelper
   {
      public static string Parameterize( this string path )
      {
         if( Settings.ApplicationName != null )
         {
            return path
               .Replace( "{lang}", Settings.Language )
               .Replace( "{Lang}", Settings.Language )
               .Replace( "{GameExeName}", Settings.ApplicationName );
         }
         return path;
      }
   }
}
