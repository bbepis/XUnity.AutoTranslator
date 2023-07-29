using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Common.Attributes;
#endif

namespace UnityEngine.Events
{
   public delegate void UnityAction<T0>( T0 arg0 );

#if IL2CPP
   public sealed class UnityAction<T0, T1> : Il2CppSystem.MulticastDelegate
   {
      private static readonly System.IntPtr NativeMethodInfoPtr_Method_Public_Void_Object_IntPtr_0;
      private static readonly System.IntPtr NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_T0_T1_0;
      private static readonly System.IntPtr NativeMethodInfoPtr_BeginInvoke_Public_Virtual_New_IAsyncResult_T0_T1_AsyncCallback_Object_0;
      private static readonly System.IntPtr NativeMethodInfoPtr_EndInvoke_Public_Virtual_New_Void_IAsyncResult_0;

      public UnityAction( Il2CppSystem.Object @object, System.IntPtr method )
        : this( IL2CPP.il2cpp_object_new( (System.IntPtr)Il2CppClassPointerStore<UnityAction<T0, T1>>.NativeClassPtr ) )
      {

      }

      [CallerCount( 0 )]
      public void Invoke( T0 arg0, T1 arg1 )
      {

      }

      [CallerCount( 0 )]
      public Il2CppSystem.IAsyncResult BeginInvoke(
        T0 arg0,
        T1 arg1,
        Il2CppSystem.AsyncCallback callback,
        Il2CppSystem.Object @object )
      {
         return null;
      }

      [CallerCount( 0 )]
      public void EndInvoke( Il2CppSystem.IAsyncResult result )
      {
      }

      static UnityAction()
      {

      }

      public UnityAction( System.IntPtr obj0 )
        : base( obj0 )
      {
      }

      public static implicit operator UnityAction<T0, T1>( System.Action<T0, T1> obj0 ) => DelegateSupport.ConvertDelegate<UnityAction<T0, T1>>( (System.Delegate)obj0 );
   }
#else
   public delegate void UnityAction<T0, T1>( T0 arg0, T1 arg1 );
#endif
}
