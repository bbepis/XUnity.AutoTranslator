using System.Collections.Generic;

namespace XUnity.ResourceRedirector
{
   internal struct BackingFieldOrArray
   {
      private UnityEngine.Object _field;
#if MANAGED
      private UnityEngine.Object[] _array;
#else
      private Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> _array;
#endif
      private BackingSource _source;

      public BackingFieldOrArray( UnityEngine.Object field )
      {
         _field = field;
         _array = null;
         _source = BackingSource.SingleField;
      }

#if MANAGED
      public BackingFieldOrArray( UnityEngine.Object[] array )
#else
      public BackingFieldOrArray( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> array )
#endif
      {
         _field = null;
         _array = array;
         _source = BackingSource.Array;
      }

      public UnityEngine.Object Field
      {
         get
         {
            if( _source == BackingSource.None )
            {
               return null;
            }
            else if( _source == BackingSource.SingleField )
            {
               return _field;
            }
            else
            {
               return _array != null && _array.Length > 0 ? _array[ 0 ] : null;
            }
         }
         set
         {
            _field = value;
            _array = null;
            _source = BackingSource.SingleField;
         }
      }

#if MANAGED
      public UnityEngine.Object[] Array
#else
      public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> Array
#endif
      {
         get
         {
            if( _source == BackingSource.Array )
            {
               return _array;
            }
            else
            {
               // create an empty array if None is correct
               if( _field == null )
               {
                  Array = new UnityEngine.Object[ 0 ];
               }
               else
               {
                  Array = new UnityEngine.Object[ 1 ] { _field };
               }

               return _array;
            }
         }
         set
         {
            _field = null;
            _array = value;
            _source = BackingSource.Array;
         }
      }

      public IEnumerable<UnityEngine.Object> IterateObjects()
      {
         if( _array != null )
         {
            foreach( var obj in _array )
            {
               yield return obj;
            }
         }
         else if( _field != null )
         {
            yield return _field;
         }
      }
   }
}
