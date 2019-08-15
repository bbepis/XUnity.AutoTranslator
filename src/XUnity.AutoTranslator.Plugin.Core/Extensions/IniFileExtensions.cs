using System;
using System.Globalization;
using System.Linq;
using ExIni;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class IniFileExtensions
   {
      public static void Set<T>( this IniFile that, string section, string key, T value )
      {
         var typeOfT = typeof( T ).UnwrapNullable();
         var iniSection = that[ section ];
         var iniKey = iniSection[ key ];

         if( value == null )
         {
            iniKey.Value = string.Empty;
         }
         else
         {
            if( typeOfT.IsEnum )
            {
               iniKey.Value = EnumHelper.GetNames( typeOfT, value );
            }
            else
            {
               iniKey.Value = Convert.ToString( value, CultureInfo.InvariantCulture );
            }
         }
      }

      public static T GetOrDefault<T>( this IniFile that, string section, string key, T defaultValue )
      {
         var typeOfT = typeof( T ).UnwrapNullable();
         var iniSection = that[ section ];
         var iniKey = iniSection[ key ];

         try
         {
            var value = iniKey.Value;

            if( value == null ) // we want to use the default value, because it is null, the config has just been created
            {
               if( defaultValue != null )
               {
                  if( typeOfT.IsEnum )
                  {
                     iniKey.Value = EnumHelper.GetNames( typeOfT, defaultValue );
                  }
                  else
                  {
                     iniKey.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
                  }
               }
               else
               {
                  iniKey.Value = string.Empty;
               }
               return defaultValue;
            }
            else
            {
               // there exists a value in the config, so we do not want to set anything
               // we just want to return what we can find, default not included
               if( !string.IsNullOrEmpty( value ) )
               {
                  if( typeOfT.IsEnum )
                  {
                     return (T)EnumHelper.GetValues( typeOfT, iniKey.Value );
                  }
                  else
                  {
                     return (T)Convert.ChangeType( iniKey.Value, typeOfT, CultureInfo.InvariantCulture );
                  }
               }
               return default( T );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, $"Error occurred while reading config '{key}' in section '{section}'. Updating the config to its default value '{defaultValue}'." );

            if( defaultValue != null )
            {
               if( typeOfT.IsEnum )
               {
                  iniKey.Value = EnumHelper.GetNames( typeOfT, defaultValue );
               }
               else
               {
                  iniKey.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
               }
            }
            else
            {
               iniKey.Value = string.Empty;
            }

            return defaultValue;
         }
      }
   }
}
