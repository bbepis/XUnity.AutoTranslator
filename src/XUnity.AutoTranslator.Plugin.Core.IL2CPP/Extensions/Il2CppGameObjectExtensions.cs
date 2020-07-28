using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class Il2CppGameObjectExtensions
   {
      public static readonly Func<GameObject, Il2CppSystem.Type, Component> GameObject_GetComponent =
         (Func<GameObject, Il2CppSystem.Type, Component>)ExpressionHelper.CreateTypedFastInvoke(
            typeof( GameObject ).GetMethod(
               "GetComponent",
               BindingFlags.Public | BindingFlags.Instance,
               null,
               new Type[] { typeof( Il2CppSystem.Type ) },
               null
            ) );

      public static Component GetFirstComponentInSelfOrAncestor( this GameObject go, Il2CppSystem.Type type )
      {
         if( type == null ) return null;

         var current = go;

         while( current != null )
         {
            var foundComponent = current.GetComponentSafe( type );
            if( foundComponent != null )
            {
               return foundComponent;
            }

            current = current.transform?.parent?.gameObject;
         }

         return null;
      }

      public static Component GetComponentSafe( this GameObject go, Il2CppSystem.Type type )
      {
         return GameObject_GetComponent( go, type );
      }
   }
}
