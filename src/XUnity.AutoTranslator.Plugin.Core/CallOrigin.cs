using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class CallOrigin
   {
      public static bool ImageHooksEnabled = true;
      public static bool ExpectsTextToBeReturned = false;
      public static IReadOnlyTextTranslationCache TextCache = null;

      private static readonly HashSet<Assembly> BreakingAssemblies;

      static CallOrigin()
      {
         BreakingAssemblies = new HashSet<Assembly>();
         try
         {
            BreakingAssemblies.AddRange(
               AppDomain.CurrentDomain
                  .GetAssemblies()
                  .Where( x => x.GetName().Name.Equals( "Assembly-CSharp" ) || x.GetName().Equals( "Assembly-CSharp-firstpass" ) )
               );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while scanning for game assemblies." );
         }
      }

      internal static IReadOnlyTextTranslationCache GetTextCache( TextTranslationInfo info, TextTranslationCache generic )
      {
         if( info != null )
         {
            return info.TextCache ?? generic;
         }
         else
         {
            return TextCache ?? generic;
         }
      }

      public static void AssociateSubHierarchyWithTransformInfo( Transform root, TransformInfo info )
      {
         var transforms = root.GetComponentsInChildren<Transform>( true );
         foreach( var transform in transforms )
         {
            transform.SetExtensionData( info );
         }

         SetTextCacheForAllObjectsInHierachy( root.gameObject, info.TextCache );
      }

      public static void SetTextCacheForAllObjectsInHierachy( this GameObject go, IReadOnlyTextTranslationCache cache )
      {
         try
         {
            foreach( var comp in go.GetAllTextComponentsInChildren() )
            {
               var derivedComp = comp.CreateDerivedProxyIfRequiredAndPossible();
               var info = derivedComp.GetOrCreateTextTranslationInfo();
               info.TextCache = cache;
            }

            var ti = new TransformInfo { TextCache = cache };
            foreach( var transform in go.GetComponentsInChildren<Transform>( true ) )
            {
               transform.SetExtensionData( ti );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while scanning object hierarchy for text components." );
         }
      }

      public static IReadOnlyTextTranslationCache CalculateTextCacheFromStackTrace( GameObject parent )
      {
         try
         {
            var trace = new StackTrace( 2 );
            var caches = AutoTranslationPlugin.Current.PluginTextCaches;
            if( caches == null ) return null;

            var frames = trace.GetFrames();
            var len = frames.Length;
            for( int i = 0; i < len; i++ )
            {
               var frame = frames[ i ];
               var method = frame.GetMethod();
               if( method != null )
               {
                  var type = method.DeclaringType;
                  var assembly = type.Assembly;
                  if( BreakingAssemblies.Contains( assembly ) )
                     break;

                  var name = assembly.GetName().Name;
                  if( caches.TryGetValue( name, out var tc ) )
                  {
                     var translationCache = AutoTranslationPlugin.Current.TextCache.GetOrCreateCompositeCache( tc );
                     return translationCache;
                  }
               }
            }

            if( parent != null )
            {
               var info = parent.transform.GetExtensionData<TransformInfo>();
               return info?.TextCache;
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while calculating text translation cache from stack trace." );
         }

         return null;
      }
   }
}
