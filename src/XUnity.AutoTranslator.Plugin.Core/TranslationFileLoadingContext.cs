using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TranslationFileLoadingContext
   {
      private static readonly Func<ResolutionCheckVariables, bool> DefaultResolutionCheck = x => true;

      private HashSet<string> _executables = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
      private HashSet<int> _levels = new HashSet<int>();
      private HashSet<string> _enabledTags = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
      private Func<ResolutionCheckVariables, bool> _resolutionCheck = DefaultResolutionCheck;

      public bool IsApplicable()
      {
         return IsValidExecutable() && _resolutionCheck( new ResolutionCheckVariables( Screen.width, Screen.height ) );
      }

      public bool IsValidExecutable()
      {
         if( _executables.Count == 0 ) return true;

         return _executables.Contains( Settings.ApplicationName );
      }

      public HashSet<int> GetLevels()
      {
         return _levels;
      }

      public bool IsEnabled( string tag )
      {
         return _enabledTags.Contains( tag );
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
            if( !Settings.EnableTranslationScoping ) return;

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

      public struct ResolutionCheckVariables
      {
         public ResolutionCheckVariables( int width, int height )
         {
            Width = width;
            Height = height;
         }

         public int Width { get; }

         public int Height { get; }
      }

      public class SetRequiredResolutionTranslationFileDirective : TranslationFileDirective
      {
         private readonly string _expression;
         private readonly Func<ResolutionCheckVariables, bool> _predicate;

         public SetRequiredResolutionTranslationFileDirective( string expression )
         {
            _expression = expression;
            _predicate = DynamicLinq.DynamicExpression.ParseLambda<ResolutionCheckVariables, bool>( expression ).Compile();
         }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            if( !Settings.EnableTranslationScoping ) return;

            context._resolutionCheck = _predicate;
         }

         public override string ToString()
         {
            return "#set required-resolution " + _expression;
         }
      }

      public class UnsetRequiredResolutionTranslationFileDirective : TranslationFileDirective
      {
         public UnsetRequiredResolutionTranslationFileDirective()
         {
         }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            if( !Settings.EnableTranslationScoping ) return;

            context._resolutionCheck = DefaultResolutionCheck;
         }

         public override string ToString()
         {
            return "#unset required-resolution";
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
            if( !Settings.EnableTranslationScoping ) return;

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
            if( !Settings.EnableTranslationScoping ) return;

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
            if( !Settings.EnableTranslationScoping ) return;

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

      public class EnableTranslationFileDirective : TranslationFileDirective
      {
         public EnableTranslationFileDirective( string tag )
         {
            Tag = tag;
         }

         public string Tag { get; }

         public override void ModifyContext( TranslationFileLoadingContext context )
         {
            if( Tag != null )
            {
               context._enabledTags.Add( Tag );
            }
         }

         public override string ToString()
         {
            return "#enable " + Tag;
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
            if( parts.Length >= 2 )
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
                  case "#enable":
                     return CreateEnableCommand( setType, argument );
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
            case "required-resolution":
               return new TranslationFileLoadingContext.SetRequiredResolutionTranslationFileDirective( argument );
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
            case "required-resolution":
               return new TranslationFileLoadingContext.UnsetRequiredResolutionTranslationFileDirective();
            case "level":
               return new TranslationFileLoadingContext.UnsetLevelTranslationFileDirective( ParseCommaSeperatedListAsIntArray( argument ) );
            case "exe":
               return new TranslationFileLoadingContext.UnsetExeTranslationFileDirective( ParseCommaSeperatedListAsStringArray( argument ) );
            default:
               break;
         }
         return null;
      }

      private static TranslationFileDirective CreateEnableCommand( string setType, string argument )
      {
         return new TranslationFileLoadingContext.EnableTranslationFileDirective( setType );
      }

      private static int[] ParseCommaSeperatedListAsIntArray( string argument )
      {
         if( string.IsNullOrEmpty( argument ) ) return new int[ 0 ];

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
         if( string.IsNullOrEmpty( argument ) ) return new string[ 0 ];

         return argument.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim() ).ToArray();
      }

      public abstract void ModifyContext( TranslationFileLoadingContext context );
   }
}
