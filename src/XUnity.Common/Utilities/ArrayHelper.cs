using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ArrayHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
#if MANAGED
      public static T[] Null<T>() => null;
#else
      public static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<T> Null<T>() where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase => null;
#endif
   }
}
