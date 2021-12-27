
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;

namespace XUnity.Common.Constants
{
   public class TypeContainer
   {
#if IL2CPP
      public TypeContainer( Il2CppSystem.Type unityType, Type clrType, IntPtr classPointer )
      {
         UnityType = unityType;
         ClrType = clrType;
         ClassPointer = classPointer;
      }
#else
      public TypeContainer( Type type )
      {
         UnityType = type;
         ClrType = type;
      }

#endif
      public Type ClrType { get; }

#if IL2CPP
      public Il2CppSystem.Type UnityType { get; }
      public IntPtr ClassPointer { get; }
#else
      public Type UnityType { get; }
#endif

#if IL2CPP
      public bool IsAssignableFrom( Il2CppSystem.Type unityType )
#else
      public bool IsAssignableFrom( Type unityType )
#endif
      {
         return UnityType != null && UnityType.IsAssignableFrom( unityType );
      }
   }
}

