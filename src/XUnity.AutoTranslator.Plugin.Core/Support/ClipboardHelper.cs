using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ClipboardHelper
   {
      private static IClipboardHelper _instance;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static IClipboardHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<IClipboardHelper>(
                  typeof( ClipboardHelper ).Assembly,
                  "XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="lines"></param>
      /// <param name="maxCharacters"></param>
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

         Instance.CopyToClipboard( text );
      }
   }

   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public interface IClipboardHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      void CopyToClipboard( string text );

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      bool SupportsClipboard();
   }
}
