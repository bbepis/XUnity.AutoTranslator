#if IL2CPP
using System.Collections.Generic;
using UnhollowerBaseLib;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public class Il2CppObjectReferenceComparer : IEqualityComparer<Il2CppObjectBase>
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static readonly Il2CppObjectReferenceComparer Default = new Il2CppObjectReferenceComparer();

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public bool Equals( Il2CppObjectBase x, Il2CppObjectBase y )
      {
         return x.Pointer == y.Pointer;
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public int GetHashCode( Il2CppObjectBase obj )
      {
         return obj.Pointer.GetHashCode();
      }
   }
}

#endif
