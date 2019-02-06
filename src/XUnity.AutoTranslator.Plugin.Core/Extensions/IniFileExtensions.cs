using System;
using System.Globalization;
using ExIni;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class IniFileExtensions
   {
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
                     iniKey.Value = Enum.GetName( typeOfT, defaultValue );
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
                     return (T)Enum.Parse( typeOfT, iniKey.Value, true );
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
            XuaLogger.Current.Error( e, $"Error occurred while reading config '{key}' in section '{section}'. Updating the config to its default value '{defaultValue}'." );

            if( defaultValue != null )
            {
               if( typeOfT.IsEnum )
               {
                  iniKey.Value = Enum.GetName( typeOfT, defaultValue );
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
