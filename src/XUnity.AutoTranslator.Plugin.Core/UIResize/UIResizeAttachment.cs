using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UIResizeAttachment
   {
      private static readonly char[] CommandSplitters = new char[] { ';' };
      private static readonly char[] PathSplitters = new char[] { '/' };
      private static readonly char[] ArgSplitters = new char[] { ',', ' ' };
      private static Regex CommandRegex = new Regex( @"^\s*(.+)\s*\(([\s\S]*)\)\s*$" );
      private static Dictionary<string, Type> CommandTypes;

      static UIResizeAttachment()
      {
         CommandTypes = new Dictionary<string, Type>( StringComparer.OrdinalIgnoreCase );

         try
         {
            var commands = new Type[]
            {
               typeof(ChangeFontSize),
               typeof(ChangeFontSizeByPercentage),
               typeof(IgnoreFontSize),
               typeof(AutoResize),
               typeof(UGUI_ChangeLineSpacing),
               typeof(UGUI_ChangeLineSpacingByPercentage),
               typeof(UGUI_HorizontalOverflow),
               typeof(UGUI_VerticalOverflow),
               typeof(TMP_Overflow),
               typeof(TMP_Alignment),
            };

            //var commands = typeof( AutoTranslationPlugin ).Assembly.GetTypes()
            //   .Where( x => ( typeof( IFontResizeCommand ).IsAssignableFrom( x )
            //   || typeof( IFontAutoResizeCommand ).IsAssignableFrom( x )
            //   || typeof( IUGUI_LineSpacingCommand ).IsAssignableFrom( x )
            //   || typeof( IUGUI_HorizontalOverflow ).IsAssignableFrom( x )
            //   || typeof( IUGUI_VerticalOverflow ).IsAssignableFrom( x )
            //   ) && !x.IsInterface && !x.IsAbstract )
            //   .ToList();

            foreach( var command in commands )
            {
               CommandTypes[ command.Name ] = command;
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while loading ui resize commands." );
         }
      }

      public UIResizeAttachment()
      {
         Descendants = new Dictionary<string, UIResizeAttachment>();
         Result = new UIResizeResult();
         ScopedResults = new Dictionary<int, UIResizeResult>();
      }

      public Dictionary<string, UIResizeAttachment> Descendants { get; }

      public Dictionary<int, UIResizeResult> ScopedResults { get; }

      public UIResizeResult Result { get; private set; }

      public bool AddResizeCommand( string path, string commands, int scope )
      {
         var segments = path.Split( PathSplitters, StringSplitOptions.RemoveEmptyEntries );
         var attachment = GetOrCreateAttachment( segments );
         var commandStrings = commands.Split( CommandSplitters, StringSplitOptions.RemoveEmptyEntries );

         bool addedAny = false;
         foreach( var commandString in commandStrings )
         {
            var match = CommandRegex.Match( commandString );
            if( !match.Success )
            {
               XuaLogger.AutoTranslator.Warn( "Could not understand command: " + commandString );
               continue;
            }

            try
            {
               var command = match.Groups[ 1 ].Value;
               var args = match.Groups[ 2 ].Value.Split( ArgSplitters, StringSplitOptions.RemoveEmptyEntries );
               var result = scope == TranslationScopes.None ? attachment.Result : attachment.GetOrCreateResultFor( scope );

               if( CommandTypes.TryGetValue( command, out var commandType ) )
               {
                  var resizeCommand = Activator.CreateInstance( commandType, new object[] { args } );
                  if( resizeCommand is IFontResizeCommand fontResizeCommand )
                  {
                     result.ResizeCommand = fontResizeCommand;
                     result.IsResizeCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is IFontAutoResizeCommand fontAutoResizeCommand )
                  {
                     result.AutoResizeCommand = fontAutoResizeCommand;
                     result.IsAutoResizeCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is IUGUI_LineSpacingCommand lineSpacingCommand )
                  {
                     result.LineSpacingCommand = lineSpacingCommand;
                     result.IsLineSpacingCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is IUGUI_HorizontalOverflow horizontalOverflowCommand )
                  {
                     result.HorizontalOverflowCommand = horizontalOverflowCommand;
                     result.IsHorizontalOverflowCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is IUGUI_VerticalOverflow verticalOverflowCommand )
                  {
                     result.VerticalOverflowCommand = verticalOverflowCommand;
                     result.IsVerticalOverflowCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is ITMP_OverflowMode overflowCommand )
                  {
                     result.OverflowCommand = overflowCommand;
                     result.IsOverflowCommandScoped = scope != TranslationScopes.None;
                  }

                  if( resizeCommand is ITMP_Alignment alignmentCommand )
                  {
                     result.AlignmentCommand = alignmentCommand;
                     result.IsAlignmentCommandScoped = scope != TranslationScopes.None;
                  }
               }
               else
               {
                  throw new ArgumentException( "Unknown command: " + command );
               }

               addedAny = true;
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while creating UI resize command." );
            }
         }

         return addedAny;
      }

      private UIResizeResult GetOrCreateResultFor( int scope )
      {
         if( !ScopedResults.TryGetValue( scope, out var result ) )
         {
            result = new UIResizeResult();
            ScopedResults[ scope ] = result;
         }
         return result;
      }

      private UIResizeAttachment GetOrCreateAttachment( string[] segments )
      {
         UIResizeAttachment attachment = this;

         var len = segments.Length;
         for( int i = 0; i < len; i++ )
         {
            var segment = segments[ i ];
            if( !attachment.Descendants.TryGetValue( segment, out var newAttachment ) )
            {
               newAttachment = new UIResizeAttachment();
               attachment.Descendants[ segment ] = newAttachment;
            }
            attachment = newAttachment;
         }

         return attachment;
      }

      public void Trim()
      {
         if( Result != null && Result.IsEmpty() )
         {
            Result = null;
         }

         foreach( var descendant in Descendants.Values )
         {
            descendant.Trim();
         }
      }

      public bool TryGetUIResize( string[] segments, int startIndex, int scope, out UIResizeResult result )
      {
         UIResizeAttachment attachment = this;
         result = null;

         var len = segments.Length;
         for( int i = startIndex; i < len; i++ )
         {
            var segment = segments[ i ];
            if( attachment.Descendants.TryGetValue( segment, out var newAttachment ) )
            {
               if( result == null )
               {
                  result = newAttachment.Result?.Copy();
               }
               else
               {
                  result.MergeInto( newAttachment.Result );
               }

               if( scope != TranslationScopes.None )
               {
                  if( result == null )
                  {
                     if( newAttachment.ScopedResults.TryGetValue( scope, out var scopedResult ) )
                     {
                        result = scopedResult.Copy();
                     }
                  }
                  else
                  {
                     if( newAttachment.ScopedResults.TryGetValue( scope, out var scopedResult ) )
                     {
                        result.MergeInto( scopedResult );
                     }
                  }
               }

               //if( attachment.HasWildcardBelow )
               //{
               //   // we have to iterate all descendants of this attachment
               //   foreach( var childAttachment in attachment.Descendants.Values )
               //   {
               //      if( childAttachment.TryGetUIResize( segments, i + 1, scope, out var otherResult ) )
               //      {
               //         if( result == null )
               //         {
               //            result = otherResult.Copy();
               //         }
               //         else
               //         {
               //            result.MergeInto( otherResult );
               //         }
               //      }
               //   }
               //}

               attachment = newAttachment;
            }
            else
            {
               return result != null;
            }
         }

         return result != null;
      }
   }
}
