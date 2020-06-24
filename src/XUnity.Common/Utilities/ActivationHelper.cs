using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   public static class ActivationHelper
   {
      public static TInstance Create<TInstance>( params string[] typeNames )
      {
         foreach( var typeName in typeNames )
         {
            var type = Type.GetType( typeName, false );
            if( type != null )
            {
               return (TInstance)Activator.CreateInstance( type );
            }
         }
         return default;
      }
   }
}
