using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ParserTranslationContext
   {
      public ParserTranslationContext( object component, TranslationEndpointManager endpoint, InternalTranslationResult translationResult, ParserResult result, ParserTranslationContext parentContext )
      {
         Jobs = new HashSet<TranslationJob>();
         ChildrenContexts = new List<ParserTranslationContext>();
         Component = component;
         Result = result;
         Endpoint = endpoint;
         TranslationResult = translationResult;
         ParentContext = parentContext;

         if( parentContext != null )
         {
            // thought: What would happen if I threw an exception after this massive anti-pattern?
            parentContext.ChildrenContexts.Add( this );
         }

         var ctx = this;
         while( ctx != null )
         {
            ctx = ctx.ParentContext;
            LevelsOfRecursion++;
         }
      }

      public ParserResult Result { get; private set; }

      public HashSet<TranslationJob> Jobs { get; private set; }

      public InternalTranslationResult TranslationResult { get; private set; }

      public object Component { get; private set; }

      public TranslationEndpointManager Endpoint { get; private set; }

      public ParserTranslationContext ParentContext { get; private set; }

      public List<ParserTranslationContext> ChildrenContexts { get; private set; }

      public int LevelsOfRecursion { get; private set; }

      private ParserResult GetHighestPriorityResult()
      {
         var highestPriorityResult = Result;
         var highestPriority = highestPriorityResult.Priority;
         var currentContext = this;

         while( ( currentContext = currentContext.ParentContext ) != null )
         {
            var result = currentContext.Result;
            var priority = result.Priority;
            if( priority > highestPriority )
            {
               highestPriority = priority;
               highestPriorityResult = result;
            }
         }

         return highestPriorityResult;
      }

      public bool CachedCombinedResult()
      {
         return GetHighestPriorityResult().CacheCombinedResult;
      }

      public bool PersistCombinedResult()
      {
         return GetHighestPriorityResult().PersistCombinedResult;
      }

      public bool HasAllJobsCompleted()
      {
         var completed = Jobs.All( x => x.State == TranslationJobState.Succeeded );
         if( completed )
         {
            foreach( var child in ChildrenContexts )
            {
               completed = child.HasAllJobsCompleted();
               if( !completed ) return false;
            }
         }
         return completed;
      }
   }

   internal static class ParserTranslationContextExtensions
   {
      public static bool HasBeenParsedBy( this ParserTranslationContext context, ParserResultOrigin parser )
      {
         var current = context;
         while( current != null )
         {
            if( current.Result.Origin == parser )
               return true;

            current = current.ParentContext;
         }
         return false;
      }

      public static int GetLevelsOfRecursion( this ParserTranslationContext context )
      {
         return context?.LevelsOfRecursion ?? 0;
      }

      public static ParserTranslationContext GetAncestorContext( this ParserTranslationContext context )
      {
         var ancestor = context;
         var parent = ancestor.ParentContext;
         while( parent != null )
         {
            ancestor = parent;
            parent = ancestor.ParentContext;
         }
         return ancestor;
      }
   }
}
