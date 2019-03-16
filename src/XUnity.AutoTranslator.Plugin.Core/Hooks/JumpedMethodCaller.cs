using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony.ILCopying;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal class JumpedMethodCaller
   {
      private readonly long _originalMethodLocation;
      private readonly long _replacementMethodLocation;
      private readonly byte[] _originalCode;

      public JumpedMethodCaller( long originalMethodLocation, long replacementMethodLocation, byte[] originalCode )
      {
         _originalMethodLocation = originalMethodLocation;
         _replacementMethodLocation = replacementMethodLocation;
         _originalCode = originalCode;
      }

      public object Func( Func<object> call )
      {
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, _originalMethodLocation, _originalCode );

            return call();
         }
         finally
         {
            MemoryHelper.WriteJump( false, _originalMethodLocation, _replacementMethodLocation );
         }
      }

      public void Action( Action call )
      {
         try
         {
            MemoryHelper.RestoreInstructionsAtLocation( false, _originalMethodLocation, _originalCode );

            call();
         }
         finally
         {
            MemoryHelper.WriteJump( false, _originalMethodLocation, _replacementMethodLocation );
         }
      }
   }
}
