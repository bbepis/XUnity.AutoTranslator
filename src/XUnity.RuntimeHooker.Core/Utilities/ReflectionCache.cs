using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.RuntimeHooker.Core.Utilities
{
   public static class ReflectionCache
   {
      private static Dictionary<MemberLookupKey, CachedMethod> Methods = new Dictionary<MemberLookupKey, CachedMethod>();
      private static Dictionary<MemberLookupKey, CachedProperty> Properties = new Dictionary<MemberLookupKey, CachedProperty>();

      public static CachedMethod CachedMethod( this Type type, string name )
      {
         var key = new MemberLookupKey( type, name );
         if( !Methods.TryGetValue( key, out var cachedMember ) )
         {
            var currentType = type;
            MethodInfo method = null;

            while( method == null && currentType != null )
            {
               method = currentType.GetMethod( name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
               currentType = currentType.BaseType;
            }

            if( method != null )
            {
               cachedMember = new CachedMethod( method );
            }

            // also cache nulls!
            Methods[ key ] = cachedMember;
         }

         return cachedMember;
      }

      public static CachedProperty CachedProperty( this Type type, string name )
      {
         var key = new MemberLookupKey( type, name );
         if( !Properties.TryGetValue( key, out var cachedMember ) )
         {
            var currentType = type;
            PropertyInfo property = null;

            while( property == null && currentType != null )
            {
               property = currentType.GetProperty( name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
               currentType = currentType.BaseType;
            }

            if( property != null )
            {
               cachedMember = new CachedProperty( property );
            }

            // also cache nulls!
            Properties[ key ] = cachedMember;
         }

         return cachedMember;
      }

      public struct MemberLookupKey
      {
         public MemberLookupKey( Type type, string memberName )
         {
            Type = type;
            MemberName = memberName;
         }

         public Type Type { get; set; }

         public string MemberName { get; set; }

         // override object.Equals
         public override bool Equals( object obj )
         {
            if( obj is MemberLookupKey key )
            {
               return Type == key.Type && MemberName == key.MemberName;
            }
            return false;
         }

         // override object.GetHashCode
         public override int GetHashCode()
         {
            return Type.GetHashCode() + MemberName.GetHashCode();
         }
      }
   }

   public class CachedMethod
   {
      private static readonly object[] Args0 = new object[ 0 ];
      private static readonly object[] Args1 = new object[ 1 ];
      private static readonly object[] Args2 = new object[ 2 ];

      private Func<object, object[], object> _invoke;

      public CachedMethod( MethodInfo method )
      {
         _invoke = ExpressionHelper.CreateFastInvoke( method );
      }

      public object Invoke( object instance, object[] arguments )
      {
         return _invoke( instance, arguments );
      }

      public object Invoke( object instance )
      {
         return _invoke( instance, Args0 );
      }

      public object Invoke( object instance, object arg1 )
      {
         try
         {
            Args1[ 0 ] = arg1;
            return _invoke( instance, Args1 );
         }
         finally
         {
            Args1[ 0 ] = null;
         }
      }

      public object Invoke( object instance, object arg1, object arg2 )
      {
         try
         {
            Args2[ 0 ] = arg1;
            Args2[ 1 ] = arg2;
            return _invoke( instance, Args2 );
         }
         finally
         {
            Args2[ 0 ] = null;
            Args2[ 1 ] = null;
         }
      }
   }

   public class CachedProperty
   {
      private static readonly object[] Args0 = new object[ 0 ];
      private static readonly object[] Args1 = new object[ 1 ];

      private Func<object, object[], object> _set;
      private Func<object, object[], object> _get;

      public CachedProperty( PropertyInfo propertyInfo )
      {
         if( propertyInfo.CanRead )
         {
            _get = ExpressionHelper.CreateFastInvoke( propertyInfo.GetGetMethod() );
         }

         if( propertyInfo.CanWrite )
         {
            _set = ExpressionHelper.CreateFastInvoke( propertyInfo.GetSetMethod() );
         }

         PropertyType = propertyInfo.PropertyType;
      }

      public Type PropertyType { get; }

      public object Set( object instance, object[] arguments )
      {
         return _set( instance, arguments );
      }

      public object Set( object instance, object arg1 )
      {
         try
         {
            Args1[ 0 ] = arg1;
            return _set( instance, Args1 );
         }
         finally
         {
            Args1[ 0 ] = null;
         }
      }

      public object Get( object instance, object[] arguments )
      {
         return _get( instance, arguments );
      }

      public object Get( object instance )
      {
         return _get( instance, Args0 );
      }
   }
}
