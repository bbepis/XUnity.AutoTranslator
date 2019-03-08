using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class GameObjectExtensions
   {
      private static GameObject[] _objects = new GameObject[ 128 ];
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

      public static string GetPath( this GameObject obj )
      {
         int i = 0;
         _objects[ i++ ] = obj;
         while( obj.transform.parent != null )
         {
            obj = obj.transform.parent.gameObject;
            _objects[ i++ ] = obj;
         }

         StringBuilder path = new StringBuilder();
         while( --i >= 0 )
         {
            path.Append( "/" ).Append( _objects[ i ].name );
            _objects[ i ] = null;
         }


         var result = path.ToString();

         return result;
      }

      public static bool HasIgnoredName( this GameObject go )
      {
         return go.name.EndsWith( DummyName ) || go.name.Contains( XuaIgnore ) || go.transform?.parent?.name.EndsWith( DummyName ) == true;
      }
   }
}
