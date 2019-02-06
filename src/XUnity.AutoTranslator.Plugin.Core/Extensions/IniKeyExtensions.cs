//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using ExIni;
//using XUnity.AutoTranslator.Plugin.Core.Extensions;

//namespace XUnity.AutoTranslator.Plugin.Core.Extensions
//{
//   internal static class IniKeyExtensions
//   {
//      public static T GetOrDefault<T>( this IniKey that, T defaultValue )
//      {
//         var typeOfT = typeof( T ).UnwrapNullable();
//         var value = that.Value;

//         if( value == null ) // we want to use the default value, because it is null, the config has just been created
//         {
//            if( defaultValue != null )
//            {
//               if( typeOfT.IsEnum )
//               {
//                  that.Value = Enum.GetName( typeOfT, defaultValue );
//               }
//               else
//               {
//                  that.Value = Convert.ToString( defaultValue, CultureInfo.InvariantCulture );
//               }
//            }
//            else
//            {
//               that.Value = string.Empty;
//            }
//            return defaultValue;
//         }
//         else
//         {
//            // there exists a value in the config, so we do not want to set anything
//            // we just want to return what we can find, default not included
//            if( !string.IsNullOrEmpty( value ) )
//            {
//               if( typeOfT.IsEnum )
//               {
//                  return (T)Enum.Parse( typeOfT, that.Value, true );
//               }
//               else
//               {
//                  return (T)Convert.ChangeType( that.Value, typeOfT, CultureInfo.InvariantCulture );
//               }
//            }
//            return default( T );
//         }
//      }
//   }
//}
