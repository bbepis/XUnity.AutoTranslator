using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TemplatedString
   {
      public TemplatedString( string template, Dictionary<string, string> arguments )
      {
         Template = template;
         Arguments = arguments;
      }

      public string Template { get; private set; }

      public Dictionary<string, string> Arguments { get; private set; }

      public string Untemplate( string text )
      {
         foreach( var kvp in Arguments )
         {
            text = text.Replace( kvp.Key, kvp.Value );
         }
         return text;
      }

      public string RepairTemplate( string translatedTemplate )
      {
         foreach( var argument in Arguments.Keys )
         {
            if( !translatedTemplate.Contains( argument ) )
            {
               var permutations = CreatePermutations( argument );
               foreach( var permutation in permutations )
               {
                  if( translatedTemplate.Contains( permutation ) )
                  {
                     translatedTemplate = translatedTemplate.Replace( permutation, argument );
                     break;
                  }
               }
            }
         }

         return translatedTemplate;
      }

      public static string[] CreatePermutations( string argument )
      {
         var b0_1 = argument.Insert( 2, " " );   // {{ A}}
         var b0_2 = argument.Insert( 3, " " );   // {{A }}

         var b1 = argument.Substring( 1 ); // {A}}
         var b1_1 = b1.Insert( 1, " " );   // { A}}
         var b1_2 = b1.Insert( 2, " " );   // {A }}

         var b2 = argument.Substring( 0, argument.Length - 1 ); // {{A}
         var b2_1 = b2.Insert( 2, " " );   // {{ A}
         var b2_2 = b2.Insert( 3, " " );   // {{A }

         var b3 = argument.Substring( 1, argument.Length - 2 ); // {A}
         var b3_1 = b3.Insert( 1, " " );   // { A}
         var b3_2 = b3.Insert( 2, " " );   // {A }

         return new string[]
         {
            b0_1,
            b0_1,
            b2,
            b2_1,
            b2_2,
            b1,
            b1_1,
            b1_2,
            b3,
            b3_1,
            b3_2,
            b0_1.ToLower(),
            b0_2.ToLower(),
            argument.ToLower(),
            b2.ToLower(),
            b2_1.ToLower(),
            b2_2.ToLower(),
            b1.ToLower(),
            b1_1.ToLower(),
            b1_2.ToLower(),
            b3.ToLower(),
            b3_1.ToLower(),
            b3_2.ToLower(),
         };
      }
   }
}
