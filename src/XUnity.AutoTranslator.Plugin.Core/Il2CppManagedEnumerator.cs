#if IL2CPP

using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class Il2CppManagedEnumerator : Il2CppSystem.Object
   {
      static Il2CppManagedEnumerator()
      {
         ClassInjector.RegisterTypeInIl2Cpp( typeof( Il2CppManagedEnumerator ), new RegisterTypeOptions
         {
            LogSuccess = true,
            Interfaces = new[] { typeof( Il2CppSystem.Collections.IEnumerator ) }
         } );
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

   internal class ManagedEnumerable : IEnumerable
   {
      private readonly Il2CppSystem.Collections.IEnumerable _enumerable;

      public ManagedEnumerable( Il2CppSystem.Collections.IEnumerable enumerable )
      {
         _enumerable = enumerable;
      }

      public IEnumerator GetEnumerator() => new ManagedEnumerator( _enumerable.GetEnumerator() );
   }

   internal class ManagedDictionaryEnumerable : IEnumerable
   {
      private object _originalDictionaryWithType;

      public ManagedDictionaryEnumerable( object obj )
      {
         _originalDictionaryWithType = obj;
      }

      public IEnumerator GetEnumerator()
      {
         var keysCollection = (Il2CppSystem.Object)_originalDictionaryWithType.GetType().GetProperty( "Keys" ).GetValue( _originalDictionaryWithType );
         var keysCollectionType = keysCollection.GetType();
         var keysEnumerator = keysCollectionType.GetMethod( "GetEnumerator" ).Invoke( keysCollection, null );

         var valuesCollection = (Il2CppSystem.Object)_originalDictionaryWithType.GetType().GetProperty( "Values" ).GetValue( _originalDictionaryWithType );
         var valuesCollectionType = valuesCollection.GetType();
         var valuesEnumerator = valuesCollectionType.GetMethod( "GetEnumerator" ).Invoke( valuesCollection, null );

         return new ManagedDictionaryEnumerator( keysEnumerator, valuesEnumerator );
      }

      public class ManagedDictionaryEnumerator : IEnumerator
      {
         private readonly object _keysEnumerator;
         private readonly object _valuesEnumerator;
         private readonly MethodInfo _moveKeyNext;
         private readonly MethodInfo _getCurrentKey;

         private readonly MethodInfo _moveValueNext;
         private readonly MethodInfo _getCurrentValue;

         public ManagedDictionaryEnumerator( object keysEnumerator, object valuesEnumerator )
         {
            _keysEnumerator = keysEnumerator;
            _valuesEnumerator = valuesEnumerator;

            _moveKeyNext = keysEnumerator.GetType().GetMethod( "MoveNext" );
            _getCurrentKey = keysEnumerator.GetType().GetProperty( "Current" ).GetGetMethod();

            _moveValueNext = valuesEnumerator.GetType().GetMethod( "MoveNext" );
            _getCurrentValue = valuesEnumerator.GetType().GetProperty( "Current" ).GetGetMethod();
         }

         public object Current
         {
            get
            {
               var key = _getCurrentKey.Invoke( _keysEnumerator, null );
               var value = _getCurrentValue.Invoke( _valuesEnumerator, null );
               return new KeyValuePair<object, object>( key, value );
            }
         }

         public bool MoveNext() => (bool)_moveKeyNext.Invoke( _keysEnumerator, null ) & (bool)_moveValueNext.Invoke( _valuesEnumerator, null );
         public void Reset() => throw new NotImplementedException();
      }
   }

   internal class ManagedEnumerator : IEnumerator
   {
      private readonly Il2CppSystem.Collections.IEnumerator _enumerator;

      public ManagedEnumerator( Il2CppSystem.Collections.IEnumerator enumerator )
      {
         _enumerator = enumerator;
      }

      public object Current => _enumerator.Current;
      public bool MoveNext() => _enumerator.MoveNext();
      public void Reset() => _enumerator.Reset();
   }
}

#endif
