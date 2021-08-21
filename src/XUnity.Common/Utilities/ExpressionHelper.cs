using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ExpressionHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="method"></param>
      /// <returns></returns>
      public static Delegate CreateTypedFastInvoke( MethodBase method )
      {
         if( method == null ) throw new ArgumentNullException( "method" );
         return CreateTypedFastInvokeUnchecked( method );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="method"></param>
      /// <returns></returns>
      public static Delegate CreateTypedFastInvokeUnchecked( MethodBase method )
      {
         if( method == null ) return null;
         if( method.IsGenericMethod ) throw new ArgumentException( "The provided method must not be generic.", "method" );

         if( method is MethodInfo methodInfo )
         {
            if( method.IsStatic )
            {
               var parameters = methodInfo.GetParameters()
                  .Select( p => Expression.Parameter( p.ParameterType, p.Name ) )
                  .ToArray();

               var call = Expression.Call( null, methodInfo, parameters );

               return Expression.Lambda( call, parameters ).Compile();
            }
            else
            {
               var parameters = methodInfo.GetParameters()
                  .Select( p => Expression.Parameter( p.ParameterType, p.Name ) )
                  .ToList();

               parameters.Insert( 0, Expression.Parameter( methodInfo.DeclaringType, "instance" ) );

               var call = Expression.Call( parameters[ 0 ], methodInfo, parameters.Skip( 1 ).ToArray() );

               return Expression.Lambda( call, parameters.ToArray() ).Compile();
            }
         }
         else if( method is ConstructorInfo constructorInfo )
         {
            var parameters = constructorInfo.GetParameters()
               .Select( p => Expression.Parameter( p.ParameterType, p.Name ) )
               .ToArray();

            var call = Expression.New( constructorInfo, parameters );

            return Expression.Lambda( call, parameters ).Compile();
         }
         else
         {
            throw new ArgumentException( "method", "This method only supports MethodInfo and ConstructorInfo." );
         }
      }
   }
}
