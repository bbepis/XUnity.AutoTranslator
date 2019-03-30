using System;
using System.Linq;
using System.Reflection;
using XUnity.RuntimeHooker.Core;
using XUnity.RuntimeHooker.Core.Utilities;

namespace XUnity.RuntimeHooker.Core
{
   public class HookCallbackInvoker : IComparable<HookCallbackInvoker>
   {
      private readonly HookMethod _hookMethod;
      private readonly MethodBase _method;
      private readonly Func<object, object[], object> _fastInvoke;
      private readonly object[] _boundParameters;
      private readonly int[] _parameterIndices;
      private readonly int _returnValueIndex;
      private readonly int _instanceIndex;

      public HookCallbackInvoker( MethodBase originalMethod, HookMethod hookMethod )
      {
         var method = hookMethod.Method;
         _hookMethod = hookMethod;
         _method = method;
         _returnValueIndex = -1;
         _instanceIndex = -1;

         var originalParameterNames = originalMethod.GetParameters().Select( x => x.Name ).ToArray();
         var callbackParameters = method.GetParameters();
         _parameterIndices = new int[ callbackParameters.Length ];
         _boundParameters = new object[ callbackParameters.Length ];

         // create 'parameter binding map' and determine if function can be fast-invoked
         bool hasRefOrOut = false;
         for( int i = 0 ; i < callbackParameters.Length ; i++ )
         {
            var callbackParameter = callbackParameters[ i ];
            if( callbackParameter.IsOut || callbackParameter.ParameterType.IsByRef )
            {
               hasRefOrOut = true;
            }

            var parameterName = callbackParameter.Name;
            if( parameterName == "__instance" )
            {
               if( originalMethod.IsStatic )
               {
                  throw new InvalidOperationException( "Cannot get instance of static method." );
               }

               _instanceIndex = i;
            }
            else if( parameterName == "__result" )
            {
               _returnValueIndex = i;
            }
            else
            {
               // find the parameter name in the originalMethod
               var index = Array.IndexOf( originalParameterNames, parameterName );
               if( index > -1 )
               {
                  _parameterIndices[ i ] = index;
               }
            }
         }

         if( !hasRefOrOut && _method is MethodInfo methodInfo )
         {
            _fastInvoke = ExpressionHelper.CreateFastInvoke( methodInfo );
         }
      }

      public bool PrefixInvoke( object[] parameters, object __instance )
      {
         var parameterIndices = _parameterIndices;
         var boundParameters = _boundParameters;
         var len = boundParameters.Length;

         try
         {
            // bind parameters from original method call to hook call
            for( int i = 0 ; i < len ; i++ )
            {
               if( i == _instanceIndex )
               {
                  boundParameters[ i ] = __instance;
               }
               else
               {
                  boundParameters[ i ] = parameters[ parameterIndices[ i ] ];
               }
            }

            object retval;
            if( _fastInvoke != null )
            {
               retval = _fastInvoke( null, boundParameters );
            }
            else
            {
               retval = _method.Invoke( null, boundParameters );

               // rebind parameters back to original array to support ref types
               for( int i = 0 ; i < len ; i++ )
               {
                  parameters[ parameterIndices[ i ] ] = boundParameters[ i ];
               }
            }

            return retval == null || (bool)retval == true;
         }
         finally
         {
            for( int i = 0 ; i < len ; i++ )
            {
               boundParameters[ i ] = null;
            }
         }
      }

      public void PostfixInvoke( object[] parameters, object __instance, ref object __result )
      {
         var parameterIndices = _parameterIndices;
         var boundParameters = _boundParameters;
         var len = boundParameters.Length;

         try
         {
            // bind parameters from original method call to hook call
            for( int i = 0 ; i < len ; i++ )
            {
               if( i == _returnValueIndex )
               {
                  boundParameters[ i ] = __result;
               }
               else if( i == _instanceIndex )
               {
                  boundParameters[ i ] = __instance;
               }
               else
               {
                  boundParameters[ i ] = parameters[ parameterIndices[ i ] ];
               }
            }

            object retval = _fastInvoke != null
               ? _fastInvoke( null, boundParameters )
               : _method.Invoke( null, boundParameters );

            if( _returnValueIndex > -1 )
            {
               __result = boundParameters[ _returnValueIndex ];
            }
         }
         finally
         {
            for( int i = 0 ; i < len ; i++ )
            {
               boundParameters[ i ] = null;
            }
         }
      }

      public int CompareTo( HookCallbackInvoker other )
      {
         return other._hookMethod.Priority.CompareTo( _hookMethod.Priority );
      }
   }
}
