using System;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.Common.Harmony
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class AccessToolsShim
   {
      private static readonly BindingFlags All = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

      private static readonly Func<Type, string, Type[], Type[], MethodInfo> AccessTools_Method;
      private static readonly Func<Type, string, PropertyInfo> AccessTools_Property;

      static AccessToolsShim()
      {
         var method = ClrTypes.AccessTools.GetMethod( "Method", All, null, new Type[] { typeof( Type ), typeof( string ), typeof( Type[] ), typeof( Type[] ) }, null );
         var property = ClrTypes.AccessTools.GetMethod( "Property", All, null, new Type[] { typeof( Type ), typeof( string ) }, null );

         AccessTools_Method = (Func<Type, string, Type[], Type[], MethodInfo>)ExpressionHelper.CreateTypedFastInvoke( method );
         AccessTools_Property = (Func<Type, string, PropertyInfo>)ExpressionHelper.CreateTypedFastInvoke( property );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="type"></param>
      /// <param name="name"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public static MethodInfo Method( Type type, string name, params Type[] parameters )
      {
         return AccessTools_Method( type, name, parameters, null );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="type"></param>
      /// <param name="name"></param>
      /// <returns></returns>
      public static PropertyInfo Property( Type type, string name )
      {
         return AccessTools_Property( type, name );
      }
   }
}
