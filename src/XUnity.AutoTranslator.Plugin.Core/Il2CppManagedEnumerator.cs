#if IL2CPP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class Il2CppManagedEnumerator : Il2CppSystem.Object
   {
      static Il2CppManagedEnumerator()
      {
         ClassInjector.RegisterTypeInIl2CppWithInterfaces<Il2CppManagedEnumerator>( true, typeof( Il2CppSystem.Collections.IEnumerator ) );
      }

      private readonly IEnumerator _enumerator;

      public Il2CppManagedEnumerator( IntPtr ptr )
         : base( ptr )
      {

      }

      public Il2CppManagedEnumerator( IEnumerator enumerator )
         : base( ClassInjector.DerivedConstructorPointer<Il2CppManagedEnumerator>() )
      {
         _enumerator = enumerator ?? throw new ArgumentNullException( nameof( enumerator ) ); ;
         ClassInjector.DerivedConstructorBody( this );
      }

      public Il2CppSystem.Object Current
      {
         get => _enumerator.Current switch
         {
            IEnumerator next => new Il2CppManagedEnumerator( next ),
            Il2CppSystem.Object il2cppObject => il2cppObject,
            null => null,
            _ => throw new NotSupportedException( $"{_enumerator.GetType()}: Unsupported type {_enumerator.Current.GetType()}" ),
         };
      }

      public bool MoveNext() => _enumerator.MoveNext();

      public void Reset() => _enumerator.Reset();
   }
}

#endif
