using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using XUnity.RuntimeHooker.Core;
using XUnity.RuntimeHooker.Core.Utilities;

namespace XUnity.RuntimeHooker.Core
{
   public static class TrampolineHandler
   {
      public static object Func( object instance, TrampolineData data )
      {
         var jumpedMethodInfo = data.JumpedMethodInfo;
         var parameters = data.Parameters;

         var prefixes = data.Prefixes;
         var prefixesLen = prefixes.Count;
         for( int i = 0 ; i < prefixesLen ; i++ )
         {
            if( !prefixes[ i ].PrefixInvoke( parameters, instance ) ) return null;
         }

         object result;
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.OriginalCode );

            var fastInvoke = data.FastInvoke;
            result = fastInvoke != null
               ? fastInvoke( instance, parameters )
               : data.Method.Invoke( instance, parameters );
         }
         finally
         {
            MemoryHelper.WriteJump( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.ReplacementMethodLocation );
         }

         var postfixes = data.Postfixes;
         var postfixesLen = postfixes.Count;
         for( int i = 0 ; i < postfixesLen ; i++ )
         {
            postfixes[ i ].PostfixInvoke( parameters, instance, ref result );
         }

         return result;
      }

      public static void Action( object instance, TrampolineData data )
      {
         var jumpedMethodInfo = data.JumpedMethodInfo;
         var parameters = data.Parameters;

         var prefixes = data.Prefixes;
         var prefixesLen = prefixes.Count;
         for( int i = 0 ; i < prefixesLen ; i++ )
         {
            if( !prefixes[ i ].PrefixInvoke( parameters, instance ) ) return;
         }
     
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.OriginalCode );

            var fastInvoke = data.FastInvoke;
            object result = fastInvoke != null
               ? fastInvoke( instance, parameters )
               : data.Method.Invoke( instance, parameters );
         }
         finally
         {
            MemoryHelper.WriteJump( false, jumpedMethodInfo.OriginalMethodLocation, jumpedMethodInfo.ReplacementMethodLocation );
         }

         object tmp = null;
         var postfixes = data.Postfixes;
         var postfixesLen = postfixes.Count;
         for( int i = 0 ; i < postfixesLen ; i++ )
         {
            postfixes[ i ].PostfixInvoke( parameters, instance, ref tmp );
         }
      }
   }
}
