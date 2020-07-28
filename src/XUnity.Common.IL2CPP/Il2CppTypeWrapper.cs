using System;

namespace XUnity.Common.Constants
{
   public class Il2CppTypeWrapper
   {
      public Il2CppTypeWrapper( Il2CppSystem.Type il2cppType, Type wrapperType, IntPtr classPointer )
      {
         Il2CppType = il2cppType;
         ProxyType = wrapperType;
         ClassPointer = classPointer;
      }

      public Il2CppSystem.Type Il2CppType { get; }
      public Type ProxyType { get; }
      public IntPtr ClassPointer { get; }
   }
}
