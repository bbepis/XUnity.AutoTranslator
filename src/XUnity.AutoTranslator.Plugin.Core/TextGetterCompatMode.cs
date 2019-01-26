using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class TextGetterCompatMode
   {
      private static readonly Assembly XUnityAutoTranslatorAssembly = typeof( TextGetterCompatMode ).Assembly;

      [MethodImpl( MethodImplOptions.NoInlining )]
      public static void ReplaceTextWithOriginal( object instance, ref string __result )
      {
         if( !Settings.TextGetterCompatibilityMode ) return;

         var tti = instance.GetTextTranslationInfo();
         if( tti.IsTranslated )
         {
            // 0. This method
            // 1. Postfix
            // 2. Harmony-related method
            // 3. Original method
            var callingMethod = new StackFrame( 3 ).GetMethod();

            var callingAssembly = callingMethod.DeclaringType.Assembly;
            if( callingAssembly == XUnityAutoTranslatorAssembly ) return;

            var originalAssembly = instance.GetType().Assembly;
            if( callingAssembly != originalAssembly ) 
            {
               // if the assembly is not the same, it may be call from the game or another mod, so replace
               __result = tti.OriginalText;
            }
         }
      }
   }
}
