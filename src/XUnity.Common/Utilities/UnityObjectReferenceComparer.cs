using System.Collections.Generic;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
#endif

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
#if IL2CPP
   public class UnityObjectReferenceComparer : IEqualityComparer<Il2CppObjectBase>
#else
   public class UnityObjectReferenceComparer : IEqualityComparer<object>
#endif
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static readonly UnityObjectReferenceComparer Default = new UnityObjectReferenceComparer();

#if IL2CPP
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public new bool Equals( object x, object y ) => Equals( (Il2CppObjectBase)x, (Il2CppObjectBase)y );

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public bool Equals( Il2CppObjectBase x, Il2CppObjectBase y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && x.Pointer == y.Pointer );
      }
#else
      public bool Equals( object x, object y )
      {
         return ReferenceEquals( x, y );
      }
#endif

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
#if IL2CPP
      public int GetHashCode( Il2CppObjectBase obj )
      {
         return obj.Pointer.GetHashCode();
      }
#else
      public int GetHashCode( object obj )
      {
         return obj.GetHashCode();
      }
#endif
   }
}
