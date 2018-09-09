using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class GameObjectExtensions
   {
      private static readonly string DummyName = "Dummy";
      private static readonly string XuaIgnore = "XUAIGNORE";

      public static Component GetFirstComponentInSelfOrAncestor( this GameObject go, Type type )
      {
         if( type == null ) return null;

         var current = go;

         while( current != null )
         {
            var foundComponent = current.GetComponent( type );
            if( foundComponent != null )
            {
               return foundComponent;
            }
            
            current = current.transform?.parent?.gameObject;
         }

         return null;
      }

      public static bool HasIgnoredName( this GameObject go )
      {
         return go.name.EndsWith( DummyName ) || go.name.Contains( XuaIgnore ) || go.transform?.parent?.name.EndsWith( DummyName ) == true;
      }
   }
}
