using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TranslationFileLoadingContext
   {
      private HashSet<string> _executables = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
      private HashSet<int> _levels = new HashSet<int>();

      public bool IsExecutable( string executable )
      {
         if( _executables.Count == 0 ) return true;

         return _executables.Contains( executable );
      }

      public HashSet<int> GetLevels()
      {
         return _levels;
      }

      public void Apply( TranslationFileDirective directive )
      {
         directive.ModifyContext( this );
      }

      public class SetLevelTranslationFileDirective : TranslationFileDirective
      {
         public SetLevelTranslationFileDirective( int[] levels )
         {
            Levels = levels;
         }

         public int[] Levels { get; }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            foreach( var level in Levels )
            {
               context._levels.Add( level );
            }
         }

         public override string ToString()
         {
            return "#set level " + string.Join( ",", Levels.Select( x => x.ToString( CultureInfo.InvariantCulture ) ).ToArray() );
         }
      }

      public class UnsetLevelTranslationFileDirective : TranslationFileDirective
      {
         public UnsetLevelTranslationFileDirective( int[] levels )
         {
            Levels = levels;
         }

         public int[] Levels { get; }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            foreach( var level in Levels )
            {
               context._levels.Remove( level );
            }
         }

         public override string ToString()
         {
            return "#unset level " + string.Join( ",", Levels.Select( x => x.ToString( CultureInfo.InvariantCulture ) ).ToArray() );
         }
      }

      public class SetExeTranslationFileDirective : TranslationFileDirective
      {
         public SetExeTranslationFileDirective( string[] executables )
         {
            Executables = executables;
         }

         public string[] Executables { get; }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            foreach( var executable in Executables )
            {
               context._executables.Add( executable );
            }
         }

         public override string ToString()
         {
            return "#set exe " + string.Join( ",", Executables );
         }
      }

      public class UnsetExeTranslationFileDirective : TranslationFileDirective
      {
         public UnsetExeTranslationFileDirective( string[] executables )
         {
            Executables = executables;
         }

         public string[] Executables { get; }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            foreach( var executable in Executables )
            {
               context._executables.Remove( executable );
            }
         }

         public override string ToString()
         {
            return "#unset exe " + string.Join( ",", Executables );
         }
      }
   }

   internal abstract class TranslationFileDirective
   {
      public static TranslationFileDirective Create( string directive )
      {
         var commentIndex = directive.IndexOf( "//" );
         if( commentIndex > -1 )
         {
            directive = directive.Substring( 0, commentIndex );
         }

         // expect three arguments: #set level 1,4,6
         directive = directive.Trim();

         if( directive.Length > 0 && directive[ 0 ] == '#' )
         {
            var parts = directive.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            if( parts.Length >= 3 )
            {
               var command = parts[ 0 ].ToLowerInvariant();

               var setType = parts[ 1 ];
               var argument = directive.Substring( directive.IndexOf( setType ) + setType.Length ).Trim();
               setType = setType.ToLowerInvariant();
               switch( command )
               {
                  case "#set":
                     return CreateSetCommand( setType, argument );
                  case "#unset":
                     return CreateUnsetCommand( setType, argument );
                  default:
                     break;
               }

            }
         }
         return null;
      }

      private static TranslationFileDirective CreateSetCommand( string setType, string argument )
      {
         switch( setType )
         {
            case "level":
               return new TranslationFileLoadingContext.SetLevelTranslationFileDirective( ParseCommaSeperatedListAsIntArray( argument ) );
            case "exe":
               return new TranslationFileLoadingContext.SetExeTranslationFileDirective( ParseCommaSeperatedListAsStringArray( argument ) );
            default:
               break;
         }
         return null;
      }

      private static TranslationFileDirective CreateUnsetCommand( string setType, string argument )
      {
         switch( setType )
         {
            case "level":
               return new TranslationFileLoadingContext.UnsetLevelTranslationFileDirective( ParseCommaSeperatedListAsIntArray( argument ) );
            case "exe":
               return new TranslationFileLoadingContext.UnsetExeTranslationFileDirective( ParseCommaSeperatedListAsStringArray( argument ) );
            default:
               break;
         }
         return null;
      }

      private static int[] ParseCommaSeperatedListAsIntArray( string argument )
      {
         List<int> result = new List<int>();
         var args = argument.Split( new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries );
         foreach( var arg in args )
         {
            if( int.TryParse( arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i ) )
            {
               result.Add( i );
            }
         }
         return result.ToArray();
      }

      private static string[] ParseCommaSeperatedListAsStringArray( string argument )
      {
         return argument.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim() ).ToArray();
      }

      public abstract void ModifyContext( TranslationFileLoadingContext context );
   }
}
