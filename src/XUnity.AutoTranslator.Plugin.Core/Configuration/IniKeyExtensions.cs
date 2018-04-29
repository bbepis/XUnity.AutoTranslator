using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ExIni;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
   public static class IniKeyExtensions
   {
      public static T GetOrDefault<T>( this IniKey that, T defaultValue, bool allowEmpty = false )
      {
         if( !allowEmpty )
         {
            var value = that.Value;
            if( string.IsNullOrEmpty( value ) )
            {
               that.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
               return defaultValue;
            }
            else
            {
               return (T)Convert.ChangeType( that.Value, typeof( T ), CultureInfo.InvariantCulture );
            }
         }
         else
         {
            var value = that.Value;
            if( value == null )
            {
               that.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
               return defaultValue;
            }
            else
            {
               return (T)Convert.ChangeType( that.Value, typeof( T ), CultureInfo.InvariantCulture );
            }
         }
      }
   }
}
