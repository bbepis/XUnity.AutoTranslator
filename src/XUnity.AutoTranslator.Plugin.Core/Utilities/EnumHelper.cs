using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class EnumHelper
   {
      public static string GetNames( Type flagType, object value )
      {
         var attr = (FlagsAttribute)flagType.GetCustomAttributes( typeof( FlagsAttribute ), false ).FirstOrDefault();
         if( attr == null )
         {
            return Enum.GetName( flagType, value );
         }
         else
         {
            var validEnumValues = Enum.GetValues( flagType );
            var stringValues = Enum.GetNames( flagType );
            var names = string.Empty;

            foreach( var stringValue in stringValues )
            {
               var parsedEnumValue = Enum.Parse( flagType, stringValue, true );
               foreach( var validEnumValue in validEnumValues )
               {
                  long validIntegerValue = Convert.ToInt64( validEnumValue );
                  long integerValue = Convert.ToInt64( value );

                  if( ( integerValue & validIntegerValue ) != 0 && Equals( parsedEnumValue, validEnumValue ) )
                  {
                     names += stringValue + ";";
                     break;
                  }
               }
            }

            if( names.EndsWith( ";" ) )
            {
               names = names.Substring( 0, names.Length - 1 );
            }

            return names;
         }
      }

      public static object GetValues( Type flagType, string commaSeparatedStringValue )
      {
         var attr = (FlagsAttribute)flagType.GetCustomAttributes( typeof( FlagsAttribute ), false ).FirstOrDefault();
         if( attr == null )
         {
            return Enum.Parse( flagType, commaSeparatedStringValue, true );
         }
         else
         {
            var stringValues = commaSeparatedStringValue.Split( new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries );
            var validEnumValues = Enum.GetValues( flagType );
            var underlyingType = Enum.GetUnderlyingType( flagType );
            long flagValues = 0;

            foreach( var stringValue in stringValues )
            {
               bool found = false;
               foreach( var validEnumValue in validEnumValues )
               {
                  var validStringValue = Enum.GetName( flagType, validEnumValue );
                  if( string.Equals( stringValue, validStringValue, StringComparison.OrdinalIgnoreCase ) )
                  {
                     var validInteger = Convert.ToInt64( validEnumValue );
                     flagValues |= validInteger;
                     found = true;
                  }
               }

               if( !found )
               {
                  throw new ArgumentException( $"Requested value '{stringValue}' was not found." );
               }
            }
            return Convert.ChangeType( flagValues, Enum.GetUnderlyingType( flagType ) );
         }
      }
   }
}
