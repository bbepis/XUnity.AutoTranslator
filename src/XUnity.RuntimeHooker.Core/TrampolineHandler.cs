using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using XUnity.RuntimeHooker.Core;
using XUnity.RuntimeHooker.Core.Utilities;

namespace XUnity.RuntimeHooker.Core
{
   public static class TrampolineHandler
   {
      private static object Null;

      public static object Func( object instance, TrampolineData data )
      {
         var jumpedMethodInfo = data.JumpedMethodInfo;
         var parameters = data.Parameters;
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.OriginalCode );

            var prefixes = data.Prefixes;
            var prefixesLen = prefixes.Count;
            for( int i = 0 ; i < prefixesLen ; i++ )
            {
               if( !prefixes[ i ].PrefixInvoke( parameters, instance ) ) return null;
            }

            var fastInvoke = data.FastInvoke;
            object result = fastInvoke != null
               ? fastInvoke( instance, parameters )
               : data.Method.Invoke( null, parameters );

            var postfixes = data.Postfixes;
            var postfixesLen = postfixes.Count;
            for( int i = 0 ; i < postfixesLen ; i++ )
            {
               postfixes[ i ].PostfixInvoke( parameters, instance, ref result );
            }

            return result;
         }
         finally
         {
            MemoryHelper.WriteJump( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.ReplacementMethodLocation );
         }
      }

      public static void Action( object instance, TrampolineData data )
      {
         var jumpedMethodInfo = data.JumpedMethodInfo;
         var parameters = data.Parameters;
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.OriginalCode );

            var prefixes = data.Prefixes;
            var prefixesLen = prefixes.Count;
            for( int i = 0 ; i < prefixesLen ; i++ )
            {
               if( !prefixes[ i ].PrefixInvoke( parameters, instance ) ) return;
            }

            var fastInvoke = data.FastInvoke;
            object result = fastInvoke != null
               ? fastInvoke( instance, parameters )
               : data.Method.Invoke( null, parameters );

            var postfixes = data.Postfixes;
            var postfixesLen = postfixes.Count;
            for( int i = 0 ; i < postfixesLen ; i++ )
            {
               postfixes[ i ].PostfixInvoke( parameters, instance, ref Null );
            }
         }
         finally
         {
            MemoryHelper.WriteJump( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.ReplacementMethodLocation );
         }
      }
   }
}
