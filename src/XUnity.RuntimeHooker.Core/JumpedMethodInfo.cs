using System;
using System.Collections.Generic;
using System.Text;

namespace XUnity.RuntimeHooker.Core
{
   public class JumpedMethodInfo
   {
      public readonly long OriginalMethodLocation;
      public readonly long ReplacementMethodLocation;
      public readonly byte[] OriginalCode;

      public JumpedMethodInfo( long originalMethodLocation, long replacementMethodLocation, byte[] originalCode )
      {
         OriginalMethodLocation = originalMethodLocation;
         ReplacementMethodLocation = replacementMethodLocation;
         OriginalCode = originalCode;
      }
   }

}
