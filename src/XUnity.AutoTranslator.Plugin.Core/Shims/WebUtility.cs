using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal static class WebUtility
   {
      private const char HIGH_SURROGATE_START = '\uD800';
      private const char LOW_SURROGATE_START = '\uDC00';
      private const char LOW_SURROGATE_END = '\uDFFF';
      private const int UNICODE_PLANE00_END = 0x00FFFF;
      private const int UNICODE_PLANE01_START = 0x10000;
      private const int UNICODE_PLANE16_END = 0x10FFFF;

      private static readonly char[] _htmlEntityEndingChars = new char[] { ';', '&' };

      private static bool StringRequiresHtmlDecoding( string s )
      {
         // this string requires html decoding only if it contains '&'
         return ( s.IndexOf( '&' ) >= 0 );
      }

      private static unsafe int IndexOfHtmlEncodingChars( string s, int startPos )
      {
         int cch = s.Length - startPos;
         fixed( char* str = s )
         {
            for( char* pch = &str[ startPos ]; cch > 0; pch++, cch-- )
            {
               char ch = *pch;
               if( ch <= '>' )
               {
                  switch( ch )
                  {
                     case '<':
                     case '>':
                     case '"':
                     case '\'':
                     case '&':
                        return s.Length - cch;
                  }
               }
            }
         }

         return -1;
      }


      public static string HtmlEncode( string value )
      {
         if( String.IsNullOrEmpty( value ) )
         {
            return value;
         }

         // Don't create string writer if we don't have nothing to encode
         int index = IndexOfHtmlEncodingChars( value, 0 );
         if( index == -1 )
         {
            return value;
         }

         StringWriter writer = new StringWriter( CultureInfo.InvariantCulture );
         HtmlEncode( value, writer );
         return writer.ToString();
      }

      public static unsafe void HtmlEncode( string value, TextWriter output )
      {
         if( value == null )
         {
            return;
         }
         if( output == null )
         {
            throw new ArgumentNullException( "output" );
         }

         int index = IndexOfHtmlEncodingChars( value, 0 );
         if( index == -1 )
         {
            output.Write( value );
            return;
         }

         int cch = value.Length - index;
         fixed( char* str = value )
         {
            char* pch = str;
            while( index-- > 0 )
            {
               output.Write( *pch++ );
            }

            for( ; cch > 0; cch--, pch++ )
            {
               char ch = *pch;
               if( ch <= '>' )
               {
                  switch( ch )
                  {
                     case '<':
                        output.Write( "&lt;" );
                        break;
                     case '>':
                        output.Write( "&gt;" );
                        break;
                     case '"':
                        output.Write( "&quot;" );
                        break;
                     case '\'':
                        output.Write( "&#39;" );
                        break;
                     case '&':
                        output.Write( "&amp;" );
                        break;
                     default:
                        output.Write( ch );
                        break;
                  }
               }
               else
               {
                  // write out the character directly
                  output.Write( ch );
               }
            }
         }
      }


      public static string HtmlDecode( string value )
      {
         if( String.IsNullOrEmpty( value ) )
         {
            return value;
         }

         // Don't create string writer if we don't have nothing to encode
         if( !StringRequiresHtmlDecoding( value ) )
         {
            return value;
         }

         StringWriter writer = new StringWriter( CultureInfo.InvariantCulture );
         HtmlDecode( value, writer );
         return writer.ToString();
      }

      private static void ConvertSmpToUtf16( uint smpChar, out char leadingSurrogate, out char trailingSurrogate )
      {
         int utf32 = (int)( smpChar - UNICODE_PLANE01_START );
         leadingSurrogate = (char)( ( utf32 / 0x400 ) + HIGH_SURROGATE_START );
         trailingSurrogate = (char)( ( utf32 % 0x400 ) + LOW_SURROGATE_START );
      }

      public static void HtmlDecode( string value, TextWriter output )
      {
         if( value == null )
         {
            return;
         }
         if( output == null )
         {
            throw new ArgumentNullException( "output" );
         }

         if( !StringRequiresHtmlDecoding( value ) )
         {
            output.Write( value );        // good as is
            return;
         }

         int l = value.Length;
         for( int i = 0; i < l; i++ )
         {
            char ch = value[ i ];

            if( ch == '&' )
            {
               // We found a '&'. Now look for the next ';' or '&'. The idea is that
               // if we find another '&' before finding a ';', then this is not an entity,
               // and the next '&' might start a real entity (VSWhidbey 275184)
               int index = value.IndexOfAny( _htmlEntityEndingChars, i + 1 );
               if( index > 0 && value[ index ] == ';' )
               {
                  string entity = value.Substring( i + 1, index - i - 1 );

                  if( entity.Length > 1 && entity[ 0 ] == '#' )
                  {
                     // The # syntax can be in decimal or hex, e.g.
                     //      &#229;  --> decimal
                     //      &#xE5;  --> same char in hex
                     // See http://www.w3.org/TR/REC-html40/charset.html#entities

                     bool parsedSuccessfully;
                     uint parsedValue;
                     if( entity[ 1 ] == 'x' || entity[ 1 ] == 'X' )
                     {
                        parsedSuccessfully = UInt32.TryParse( entity.Substring( 2 ), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out parsedValue );
                     }
                     else
                     {
                        parsedSuccessfully = UInt32.TryParse( entity.Substring( 1 ), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsedValue );
                     }

                     if( parsedSuccessfully )
                     {
                        parsedSuccessfully = ( 0 < parsedValue && parsedValue <= UNICODE_PLANE00_END );
                     }

                     if( parsedSuccessfully )
                     {
                        if( parsedValue <= UNICODE_PLANE00_END )
                        {
                           // single character
                           output.Write( (char)parsedValue );
                        }
                        else
                        {
                           // multi-character
                           char leadingSurrogate, trailingSurrogate;
                           ConvertSmpToUtf16( parsedValue, out leadingSurrogate, out trailingSurrogate );
                           output.Write( leadingSurrogate );
                           output.Write( trailingSurrogate );
                        }

                        i = index; // already looked at everything until semicolon
                        continue;
                     }
                  }
                  else
                  {
                     i = index; // already looked at everything until semicolon

                     char entityChar = HtmlEntities.Lookup( entity );
                     if( entityChar != (char)0 )
                     {
                        ch = entityChar;
                     }
                     else
                     {
                        output.Write( '&' );
                        output.Write( entity );
                        output.Write( ';' );
                        continue;
                     }
                  }

               }
            }

            output.Write( ch );
         }
      }

      // helper class for lookup of HTML encoding entities
      private static class HtmlEntities
      {

         // The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html, except for &apos;, which
         // is defined in http://www.w3.org/TR/2008/REC-xml-20081126/#sec-predefined-ent.

         private static String[] _entitiesList = new String[] {
                "\x0022-quot",
                "\x0026-amp",
                "\x0027-apos",
                "\x003c-lt",
                "\x003e-gt",
                "\x00a0-nbsp",
                "\x00a1-iexcl",
                "\x00a2-cent",
                "\x00a3-pound",
                "\x00a4-curren",
                "\x00a5-yen",
                "\x00a6-brvbar",
                "\x00a7-sect",
                "\x00a8-uml",
                "\x00a9-copy",
                "\x00aa-ordf",
                "\x00ab-laquo",
                "\x00ac-not",
                "\x00ad-shy",
                "\x00ae-reg",
                "\x00af-macr",
                "\x00b0-deg",
                "\x00b1-plusmn",
                "\x00b2-sup2",
                "\x00b3-sup3",
                "\x00b4-acute",
                "\x00b5-micro",
                "\x00b6-para",
                "\x00b7-middot",
                "\x00b8-cedil",
                "\x00b9-sup1",
                "\x00ba-ordm",
                "\x00bb-raquo",
                "\x00bc-frac14",
                "\x00bd-frac12",
                "\x00be-frac34",
                "\x00bf-iquest",
                "\x00c0-Agrave",
                "\x00c1-Aacute",
                "\x00c2-Acirc",
                "\x00c3-Atilde",
                "\x00c4-Auml",
                "\x00c5-Aring",
                "\x00c6-AElig",
                "\x00c7-Ccedil",
                "\x00c8-Egrave",
                "\x00c9-Eacute",
                "\x00ca-Ecirc",
                "\x00cb-Euml",
                "\x00cc-Igrave",
                "\x00cd-Iacute",
                "\x00ce-Icirc",
                "\x00cf-Iuml",
                "\x00d0-ETH",
                "\x00d1-Ntilde",
                "\x00d2-Ograve",
                "\x00d3-Oacute",
                "\x00d4-Ocirc",
                "\x00d5-Otilde",
                "\x00d6-Ouml",
                "\x00d7-times",
                "\x00d8-Oslash",
                "\x00d9-Ugrave",
                "\x00da-Uacute",
                "\x00db-Ucirc",
                "\x00dc-Uuml",
                "\x00dd-Yacute",
                "\x00de-THORN",
                "\x00df-szlig",
                "\x00e0-agrave",
                "\x00e1-aacute",
                "\x00e2-acirc",
                "\x00e3-atilde",
                "\x00e4-auml",
                "\x00e5-aring",
                "\x00e6-aelig",
                "\x00e7-ccedil",
                "\x00e8-egrave",
                "\x00e9-eacute",
                "\x00ea-ecirc",
                "\x00eb-euml",
                "\x00ec-igrave",
                "\x00ed-iacute",
                "\x00ee-icirc",
                "\x00ef-iuml",
                "\x00f0-eth",
                "\x00f1-ntilde",
                "\x00f2-ograve",
                "\x00f3-oacute",
                "\x00f4-ocirc",
                "\x00f5-otilde",
                "\x00f6-ouml",
                "\x00f7-divide",
                "\x00f8-oslash",
                "\x00f9-ugrave",
                "\x00fa-uacute",
                "\x00fb-ucirc",
                "\x00fc-uuml",
                "\x00fd-yacute",
                "\x00fe-thorn",
                "\x00ff-yuml",
                "\x0152-OElig",
                "\x0153-oelig",
                "\x0160-Scaron",
                "\x0161-scaron",
                "\x0178-Yuml",
                "\x0192-fnof",
                "\x02c6-circ",
                "\x02dc-tilde",
                "\x0391-Alpha",
                "\x0392-Beta",
                "\x0393-Gamma",
                "\x0394-Delta",
                "\x0395-Epsilon",
                "\x0396-Zeta",
                "\x0397-Eta",
                "\x0398-Theta",
                "\x0399-Iota",
                "\x039a-Kappa",
                "\x039b-Lambda",
                "\x039c-Mu",
                "\x039d-Nu",
                "\x039e-Xi",
                "\x039f-Omicron",
                "\x03a0-Pi",
                "\x03a1-Rho",
                "\x03a3-Sigma",
                "\x03a4-Tau",
                "\x03a5-Upsilon",
                "\x03a6-Phi",
                "\x03a7-Chi",
                "\x03a8-Psi",
                "\x03a9-Omega",
                "\x03b1-alpha",
                "\x03b2-beta",
                "\x03b3-gamma",
                "\x03b4-delta",
                "\x03b5-epsilon",
                "\x03b6-zeta",
                "\x03b7-eta",
                "\x03b8-theta",
                "\x03b9-iota",
                "\x03ba-kappa",
                "\x03bb-lambda",
                "\x03bc-mu",
                "\x03bd-nu",
                "\x03be-xi",
                "\x03bf-omicron",
                "\x03c0-pi",
                "\x03c1-rho",
                "\x03c2-sigmaf",
                "\x03c3-sigma",
                "\x03c4-tau",
                "\x03c5-upsilon",
                "\x03c6-phi",
                "\x03c7-chi",
                "\x03c8-psi",
                "\x03c9-omega",
                "\x03d1-thetasym",
                "\x03d2-upsih",
                "\x03d6-piv",
                "\x2002-ensp",
                "\x2003-emsp",
                "\x2009-thinsp",
                "\x200c-zwnj",
                "\x200d-zwj",
                "\x200e-lrm",
                "\x200f-rlm",
                "\x2013-ndash",
                "\x2014-mdash",
                "\x2018-lsquo",
                "\x2019-rsquo",
                "\x201a-sbquo",
                "\x201c-ldquo",
                "\x201d-rdquo",
                "\x201e-bdquo",
                "\x2020-dagger",
                "\x2021-Dagger",
                "\x2022-bull",
                "\x2026-hellip",
                "\x2030-permil",
                "\x2032-prime",
                "\x2033-Prime",
                "\x2039-lsaquo",
                "\x203a-rsaquo",
                "\x203e-oline",
                "\x2044-frasl",
                "\x20ac-euro",
                "\x2111-image",
                "\x2118-weierp",
                "\x211c-real",
                "\x2122-trade",
                "\x2135-alefsym",
                "\x2190-larr",
                "\x2191-uarr",
                "\x2192-rarr",
                "\x2193-darr",
                "\x2194-harr",
                "\x21b5-crarr",
                "\x21d0-lArr",
                "\x21d1-uArr",
                "\x21d2-rArr",
                "\x21d3-dArr",
                "\x21d4-hArr",
                "\x2200-forall",
                "\x2202-part",
                "\x2203-exist",
                "\x2205-empty",
                "\x2207-nabla",
                "\x2208-isin",
                "\x2209-notin",
                "\x220b-ni",
                "\x220f-prod",
                "\x2211-sum",
                "\x2212-minus",
                "\x2217-lowast",
                "\x221a-radic",
                "\x221d-prop",
                "\x221e-infin",
                "\x2220-ang",
                "\x2227-and",
                "\x2228-or",
                "\x2229-cap",
                "\x222a-cup",
                "\x222b-int",
                "\x2234-there4",
                "\x223c-sim",
                "\x2245-cong",
                "\x2248-asymp",
                "\x2260-ne",
                "\x2261-equiv",
                "\x2264-le",
                "\x2265-ge",
                "\x2282-sub",
                "\x2283-sup",
                "\x2284-nsub",
                "\x2286-sube",
                "\x2287-supe",
                "\x2295-oplus",
                "\x2297-otimes",
                "\x22a5-perp",
                "\x22c5-sdot",
                "\x2308-lceil",
                "\x2309-rceil",
                "\x230a-lfloor",
                "\x230b-rfloor",
                "\x2329-lang",
                "\x232a-rang",
                "\x25ca-loz",
                "\x2660-spades",
                "\x2663-clubs",
                "\x2665-hearts",
                "\x2666-diams",
            };

         private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

         private static Dictionary<string, char> GenerateLookupTable()
         {
            // e[0] is unicode char, e[1] is '-', e[2+] is entity string

            Dictionary<string, char> lookupTable = new Dictionary<string, char>( StringComparer.Ordinal );
            foreach( string e in _entitiesList )
            {
               lookupTable.Add( e.Substring( 2 ), e[ 0 ] );
            }

            return lookupTable;
         }

         public static char Lookup( string entity )
         {
            char theChar;
            _lookupTable.TryGetValue( entity, out theChar );
            return theChar;
         }
      }
   }
}
