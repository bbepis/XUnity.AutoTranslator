using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ExIni;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
   public static class IniKeyExtensions
   {
      public static T GetOrDefault<T>( this IniKey that, T defaultValue, bool allowEmpty = false )
      {
         var typeOfT = typeof( T ).UnwrapNullable();
         if( !allowEmpty )
         {
            var value = that.Value;
            if( string.IsNullOrEmpty( value ) )
            {
               if( defaultValue != null )
               {
                  if( typeOfT.IsEnum )
                  {
                     that.Value = Enum.GetName( typeOfT, defaultValue );
                  }
                  else
                  {
                     that.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
                  }
               }
               else
               {
                  that.Value = string.Empty;
               }
               return defaultValue;
            }
            else
            {
               if( typeOfT.IsEnum )
               {
                  return (T)Enum.Parse( typeOfT, that.Value, true );
               }
               else
               {
                  return (T)Convert.ChangeType( that.Value, typeOfT, CultureInfo.InvariantCulture );
               }
            }
         }
         else
         {
            var value = that.Value;
            if( string.IsNullOrEmpty( value ) )
            {
               if( defaultValue != null )
               {
                  if( typeOfT.IsEnum )
                  {
                     that.Value = Enum.GetName( typeOfT, defaultValue );
                  }
                  else
                  {
                     that.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
                  }
               }
               else
               {
                  that.Value = string.Empty;
               }
               return defaultValue;
            }
            else
            {
               if( typeOfT.IsEnum )
               {
                  return (T)Enum.Parse( typeOfT, that.Value, true );
               }
               else
               {
                  return (T)Convert.ChangeType( that.Value, typeOfT, CultureInfo.InvariantCulture );
               }
            }
         }
      }
   }
}
