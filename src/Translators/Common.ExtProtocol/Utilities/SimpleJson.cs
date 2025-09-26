﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

/*
* The MIT License( MIT)
*
* Copyright( c) 2012-2017 Markus Göbel( Bunny83)
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
* 
* * * * */
namespace SimpleJSON
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
   public enum JSONNodeType
   {
      Array = 1,
      Object = 2,
      String = 3,
      Number = 4,
      NullValue = 5,
      Boolean = 6,
      None = 7,
      Custom = 0xFF,
   }
   public enum JSONTextMode
   {
      Compact,
      Indent
   }

   public abstract partial class JSONNode
   {
      #region Enumerators
      public struct Enumerator
      {
         private enum Type { None, Array, Object }
         private Type type;
         private Dictionary<string, JSONNode>.Enumerator m_Object;
         private List<JSONNode>.Enumerator m_Array;
         public bool IsValid { get { return type != Type.None; } }
         public Enumerator( List<JSONNode>.Enumerator aArrayEnum )
         {
            type = Type.Array;
            m_Object = default( Dictionary<string, JSONNode>.Enumerator );
            m_Array = aArrayEnum;
         }
         public Enumerator( Dictionary<string, JSONNode>.Enumerator aDictEnum )
         {
            type = Type.Object;
            m_Object = aDictEnum;
            m_Array = default( List<JSONNode>.Enumerator );
         }
         public KeyValuePair<string, JSONNode> Current
         {
            get
            {
               if( type == Type.Array )
                  return new KeyValuePair<string, JSONNode>( string.Empty, m_Array.Current );
               else if( type == Type.Object )
                  return m_Object.Current;
               return new KeyValuePair<string, JSONNode>( string.Empty, null );
            }
         }
         public bool MoveNext()
         {
            if( type == Type.Array )
               return m_Array.MoveNext();
            else if( type == Type.Object )
               return m_Object.MoveNext();
            return false;
         }
      }
      public struct ValueEnumerator
      {
         private Enumerator m_Enumerator;
         public ValueEnumerator( List<JSONNode>.Enumerator aArrayEnum ) : this( new Enumerator( aArrayEnum ) ) { }
         public ValueEnumerator( Dictionary<string, JSONNode>.Enumerator aDictEnum ) : this( new Enumerator( aDictEnum ) ) { }
         public ValueEnumerator( Enumerator aEnumerator ) { m_Enumerator = aEnumerator; }
         public JSONNode Current { get { return m_Enumerator.Current.Value; } }
         public bool MoveNext() { return m_Enumerator.MoveNext(); }
         public ValueEnumerator GetEnumerator() { return this; }
      }
      public struct KeyEnumerator
      {
         private Enumerator m_Enumerator;
         public KeyEnumerator( List<JSONNode>.Enumerator aArrayEnum ) : this( new Enumerator( aArrayEnum ) ) { }
         public KeyEnumerator( Dictionary<string, JSONNode>.Enumerator aDictEnum ) : this( new Enumerator( aDictEnum ) ) { }
         public KeyEnumerator( Enumerator aEnumerator ) { m_Enumerator = aEnumerator; }
         public JSONNode Current { get { return m_Enumerator.Current.Key; } }
         public bool MoveNext() { return m_Enumerator.MoveNext(); }
         public KeyEnumerator GetEnumerator() { return this; }
      }

      public class LinqEnumerator : IEnumerator<KeyValuePair<string, JSONNode>>, IEnumerable<KeyValuePair<string, JSONNode>>
      {
         private JSONNode m_Node;
         private Enumerator m_Enumerator;
         internal LinqEnumerator( JSONNode aNode )
         {
            m_Node = aNode;
            if( m_Node != null )
               m_Enumerator = m_Node.GetEnumerator();
         }
         public KeyValuePair<string, JSONNode> Current { get { return m_Enumerator.Current; } }
         object IEnumerator.Current { get { return m_Enumerator.Current; } }
         public bool MoveNext() { return m_Enumerator.MoveNext(); }

         public void Dispose()
         {
            m_Node = null;
            m_Enumerator = new Enumerator();
         }

         public IEnumerator<KeyValuePair<string, JSONNode>> GetEnumerator()
         {
            return new LinqEnumerator( m_Node );
         }

         public void Reset()
         {
            if( m_Node != null )
               m_Enumerator = m_Node.GetEnumerator();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new LinqEnumerator( m_Node );
         }
      }

      #endregion Enumerators

      #region common interface

      public static bool forceASCII = false; // Use Unicode by default

      public abstract JSONNodeType Tag { get; }

      public virtual JSONNode this[ int aIndex ] { get { return null; } set { } }

      public virtual JSONNode this[ string aKey ] { get { return null; } set { } }

      public virtual string Value { get { return ""; } set { } }

      public virtual int Count { get { return 0; } }

      public virtual bool IsNumber { get { return false; } }
      public virtual bool IsString { get { return false; } }
      public virtual bool IsBoolean { get { return false; } }
      public virtual bool IsNull { get { return false; } }
      public virtual bool IsArray { get { return false; } }
      public virtual bool IsObject { get { return false; } }

      public virtual bool Inline { get { return false; } set { } }

      public virtual void Add( string aKey, JSONNode aItem )
      {
      }
      public virtual void Add( JSONNode aItem )
      {
         Add( "", aItem );
      }

      public virtual JSONNode Remove( string aKey )
      {
         return null;
      }

      public virtual JSONNode Remove( int aIndex )
      {
         return null;
      }

      public virtual JSONNode Remove( JSONNode aNode )
      {
         return aNode;
      }

      public virtual IEnumerable<JSONNode> Children
      {
         get
         {
            yield break;
         }
      }

      public IEnumerable<JSONNode> DeepChildren
      {
         get
         {
            foreach( var C in Children )
               foreach( var D in C.DeepChildren )
                  yield return D;
         }
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         WriteToStringBuilder( sb, 0, 0, JSONTextMode.Compact );
         return sb.ToString();
      }

      public virtual string ToString( int aIndent )
      {
         StringBuilder sb = new StringBuilder();
         WriteToStringBuilder( sb, 0, aIndent, JSONTextMode.Indent );
         return sb.ToString();
      }
      internal abstract void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode );

      public abstract Enumerator GetEnumerator();
      public IEnumerable<KeyValuePair<string, JSONNode>> Linq { get { return new LinqEnumerator( this ); } }
      public KeyEnumerator Keys { get { return new KeyEnumerator( GetEnumerator() ); } }
      public ValueEnumerator Values { get { return new ValueEnumerator( GetEnumerator() ); } }

      #endregion common interface

      #region typecasting properties


      public virtual double AsDouble
      {
         get
         {
            double v = 0.0;
            if( double.TryParse( Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v ) )
               return v;
            return 0.0;
         }
         set
         {
            Value = value.ToString(CultureInfo.InvariantCulture);
         }
      }

      public virtual int AsInt
      {
         get { return (int)AsDouble; }
         set { AsDouble = value; }
      }

      public virtual float AsFloat
      {
         get { return (float)AsDouble; }
         set { AsDouble = value; }
      }

      public virtual bool AsBool
      {
         get
         {
            bool v = false;
            if( bool.TryParse( Value, out v ) )
               return v;
            return !string.IsNullOrEmpty( Value );
         }
         set
         {
            Value = ( value ) ? "true" : "false";
         }
      }

      public virtual JSONArray AsArray
      {
         get
         {
            return this as JSONArray;
         }
      }

      public virtual JSONObject AsObject
      {
         get
         {
            return this as JSONObject;
         }
      }


      #endregion typecasting properties

      #region operators

      public static implicit operator JSONNode( string s )
      {
         return new JSONString( s );
      }
      public static implicit operator string( JSONNode d )
      {
         return ( d == null ) ? null : d.Value;
      }

      public static implicit operator JSONNode( double n )
      {
         return new JSONNumber( n );
      }
      public static implicit operator double( JSONNode d )
      {
         return ( d == null ) ? 0 : d.AsDouble;
      }

      public static implicit operator JSONNode( float n )
      {
         return new JSONNumber( n );
      }
      public static implicit operator float( JSONNode d )
      {
         return ( d == null ) ? 0 : d.AsFloat;
      }

      public static implicit operator JSONNode( int n )
      {
         return new JSONNumber( n );
      }
      public static implicit operator int( JSONNode d )
      {
         return ( d == null ) ? 0 : d.AsInt;
      }

      public static implicit operator JSONNode( bool b )
      {
         return new JSONBool( b );
      }
      public static implicit operator bool( JSONNode d )
      {
         return ( d == null ) ? false : d.AsBool;
      }

      public static implicit operator JSONNode( KeyValuePair<string, JSONNode> aKeyValue )
      {
         return aKeyValue.Value;
      }

      public static bool operator ==( JSONNode a, object b )
      {
         if( ReferenceEquals( a, b ) )
            return true;
         bool aIsNull = a is JSONNull || ReferenceEquals( a, null ) || a is JSONLazyCreator;
         bool bIsNull = b is JSONNull || ReferenceEquals( b, null ) || b is JSONLazyCreator;
         if( aIsNull && bIsNull )
            return true;
         return !aIsNull && a.Equals( b );
      }

      public static bool operator !=( JSONNode a, object b )
      {
         return !( a == b );
      }

      public override bool Equals( object obj )
      {
         return ReferenceEquals( this, obj );
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      #endregion operators

      [ThreadStatic]
      private static StringBuilder m_EscapeBuilder;
      internal static StringBuilder EscapeBuilder
      {
         get
         {
            if( m_EscapeBuilder == null )
               m_EscapeBuilder = new StringBuilder();
            return m_EscapeBuilder;
         }
      }
      internal static string Escape( string aText )
      {
         var sb = EscapeBuilder;
         sb.Length = 0;
         if( sb.Capacity < aText.Length + aText.Length / 10 )
            sb.Capacity = aText.Length + aText.Length / 10;
         foreach( char c in aText )
         {
            switch( c )
            {
               case '\\':
                  sb.Append( "\\\\" );
                  break;
               case '\"':
                  sb.Append( "\\\"" );
                  break;
               case '\n':
                  sb.Append( "\\n" );
                  break;
               case '\r':
                  sb.Append( "\\r" );
                  break;
               case '\t':
                  sb.Append( "\\t" );
                  break;
               case '\b':
                  sb.Append( "\\b" );
                  break;
               case '\f':
                  sb.Append( "\\f" );
                  break;
               default:
                  if( c < ' ' || ( forceASCII && c > 127 ) )
                  {
                     ushort val = c;
                     sb.Append( "\\u" ).Append( val.ToString( "X4" ) );
                  }
                  else
                     sb.Append( c );
                  break;
            }
         }
         string result = sb.ToString();
         sb.Length = 0;
         return result;
      }

      static void ParseElement( JSONNode ctx, string token, string tokenName, bool quoted )
      {
         if( quoted )
         {
            ctx.Add( tokenName, token );
            return;
         }
         string tmp = token.ToLower();
         if( tmp == "false" || tmp == "true" )
            ctx.Add( tokenName, tmp == "true" );
         else if( tmp == "null" )
            ctx.Add( tokenName, null );
         else
         {
            double val;
            if( double.TryParse( token, out val ) )
               ctx.Add( tokenName, val );
            else
               ctx.Add( tokenName, token );
         }
      }

      public static JSONNode Parse( string aJSON )
      {
         Stack<JSONNode> stack = new Stack<JSONNode>();
         JSONNode ctx = null;
         int i = 0;
         StringBuilder Token = new StringBuilder();
         string TokenName = "";
         bool QuoteMode = false;
         bool TokenIsQuoted = false;
         while( i < aJSON.Length )
         {
            switch( aJSON[ i ] )
            {
               case '{':
                  if( QuoteMode )
                  {
                     Token.Append( aJSON[ i ] );
                     break;
                  }
                  stack.Push( new JSONObject() );
                  if( ctx != null )
                  {
                     ctx.Add( TokenName, stack.Peek() );
                  }
                  TokenName = "";
                  Token.Length = 0;
                  ctx = stack.Peek();
                  break;

               case '[':
                  if( QuoteMode )
                  {
                     Token.Append( aJSON[ i ] );
                     break;
                  }

                  stack.Push( new JSONArray() );
                  if( ctx != null )
                  {
                     ctx.Add( TokenName, stack.Peek() );
                  }
                  TokenName = "";
                  Token.Length = 0;
                  ctx = stack.Peek();
                  break;

               case '}':
               case ']':
                  if( QuoteMode )
                  {

                     Token.Append( aJSON[ i ] );
                     break;
                  }
                  if( stack.Count == 0 )
                     throw new Exception( "JSON Parse: Too many closing brackets" );

                  stack.Pop();
                  if( Token.Length > 0 || TokenIsQuoted )
                  {
                     ParseElement( ctx, Token.ToString(), TokenName, TokenIsQuoted );
                     TokenIsQuoted = false;
                  }
                  TokenName = "";
                  Token.Length = 0;
                  if( stack.Count > 0 )
                     ctx = stack.Peek();
                  break;

               case ':':
                  if( QuoteMode )
                  {
                     Token.Append( aJSON[ i ] );
                     break;
                  }
                  TokenName = Token.ToString();
                  Token.Length = 0;
                  TokenIsQuoted = false;
                  break;

               case '"':
                  QuoteMode ^= true;
                  TokenIsQuoted |= QuoteMode;
                  break;

               case ',':
                  if( QuoteMode )
                  {
                     Token.Append( aJSON[ i ] );
                     break;
                  }
                  if( Token.Length > 0 || TokenIsQuoted )
                  {
                     ParseElement( ctx, Token.ToString(), TokenName, TokenIsQuoted );
                     TokenIsQuoted = false;
                  }
                  TokenName = "";
                  Token.Length = 0;
                  TokenIsQuoted = false;
                  break;

               case '\r':
               case '\n':
                  break;

               case ' ':
               case '\t':
                  if( QuoteMode )
                     Token.Append( aJSON[ i ] );
                  break;

               case '\\':
                  ++i;
                  if( QuoteMode )
                  {
                     char C = aJSON[ i ];
                     switch( C )
                     {
                        case 't':
                           Token.Append( '\t' );
                           break;
                        case 'r':
                           Token.Append( '\r' );
                           break;
                        case 'n':
                           Token.Append( '\n' );
                           break;
                        case 'b':
                           Token.Append( '\b' );
                           break;
                        case 'f':
                           Token.Append( '\f' );
                           break;
                        case 'u':
                           {
                              string s = aJSON.Substring( i + 1, 4 );
                              Token.Append( (char)int.Parse(
                                  s,
                                  System.Globalization.NumberStyles.AllowHexSpecifier ) );
                              i += 4;
                              break;
                           }
                        default:
                           Token.Append( C );
                           break;
                     }
                  }
                  break;

               default:
                  Token.Append( aJSON[ i ] );
                  break;
            }
            ++i;
         }
         if( QuoteMode )
         {
            throw new Exception( "JSON Parse: Quotation marks seems to be messed up." );
         }
         return ctx;
      }

   }
   // End of JSONNode

   public partial class JSONArray : JSONNode
   {
      private List<JSONNode> m_List = new List<JSONNode>();
      private bool inline = false;
      public override bool Inline
      {
         get { return inline; }
         set { inline = value; }
      }

      public override JSONNodeType Tag { get { return JSONNodeType.Array; } }
      public override bool IsArray { get { return true; } }
      public override Enumerator GetEnumerator() { return new Enumerator( m_List.GetEnumerator() ); }

      public override JSONNode this[ int aIndex ]
      {
         get
         {
            if( aIndex < 0 || aIndex >= m_List.Count )
               return new JSONLazyCreator( this );
            return m_List[ aIndex ];
         }
         set
         {
            if( value == null )
               value = JSONNull.CreateOrGet();
            if( aIndex < 0 || aIndex >= m_List.Count )
               m_List.Add( value );
            else
               m_List[ aIndex ] = value;
         }
      }

      public override JSONNode this[ string aKey ]
      {
         get { return new JSONLazyCreator( this ); }
         set
         {
            if( value == null )
               value = JSONNull.CreateOrGet();
            m_List.Add( value );
         }
      }

      public override int Count
      {
         get { return m_List.Count; }
      }

      public override void Add( string aKey, JSONNode aItem )
      {
         if( aItem == null )
            aItem = JSONNull.CreateOrGet();
         m_List.Add( aItem );
      }

      public override JSONNode Remove( int aIndex )
      {
         if( aIndex < 0 || aIndex >= m_List.Count )
            return null;
         JSONNode tmp = m_List[ aIndex ];
         m_List.RemoveAt( aIndex );
         return tmp;
      }

      public override JSONNode Remove( JSONNode aNode )
      {
         m_List.Remove( aNode );
         return aNode;
      }

      public override IEnumerable<JSONNode> Children
      {
         get
         {
            foreach( JSONNode N in m_List )
               yield return N;
         }
      }


      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( '[' );
         int count = m_List.Count;
         if( inline )
            aMode = JSONTextMode.Compact;
         for( int i = 0; i < count; i++ )
         {
            if( i > 0 )
               aSB.Append( ',' );
            if( aMode == JSONTextMode.Indent )
               aSB.AppendLine();

            if( aMode == JSONTextMode.Indent )
               aSB.Append( ' ', aIndent + aIndentInc );
            m_List[ i ].WriteToStringBuilder( aSB, aIndent + aIndentInc, aIndentInc, aMode );
         }
         if( aMode == JSONTextMode.Indent )
            aSB.AppendLine().Append( ' ', aIndent );
         aSB.Append( ']' );
      }
   }
   // End of JSONArray

   public partial class JSONObject : JSONNode
   {
      private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

      private bool inline = false;
      public override bool Inline
      {
         get { return inline; }
         set { inline = value; }
      }

      public override JSONNodeType Tag { get { return JSONNodeType.Object; } }
      public override bool IsObject { get { return true; } }

      public override Enumerator GetEnumerator() { return new Enumerator( m_Dict.GetEnumerator() ); }


      public override JSONNode this[ string aKey ]
      {
         get
         {
            if( m_Dict.ContainsKey( aKey ) )
               return m_Dict[ aKey ];
            else
               return new JSONLazyCreator( this, aKey );
         }
         set
         {
            if( value == null )
               value = JSONNull.CreateOrGet();
            if( m_Dict.ContainsKey( aKey ) )
               m_Dict[ aKey ] = value;
            else
               m_Dict.Add( aKey, value );
         }
      }

      public override JSONNode this[ int aIndex ]
      {
         get
         {
            if( aIndex < 0 || aIndex >= m_Dict.Count )
               return null;
            return m_Dict.ElementAt( aIndex ).Value;
         }
         set
         {
            if( value == null )
               value = JSONNull.CreateOrGet();
            if( aIndex < 0 || aIndex >= m_Dict.Count )
               return;
            string key = m_Dict.ElementAt( aIndex ).Key;
            m_Dict[ key ] = value;
         }
      }

      public override int Count
      {
         get { return m_Dict.Count; }
      }

      public override void Add( string aKey, JSONNode aItem )
      {
         if( aItem == null )
            aItem = JSONNull.CreateOrGet();

         if( !string.IsNullOrEmpty( aKey ) )
         {
            if( m_Dict.ContainsKey( aKey ) )
               m_Dict[ aKey ] = aItem;
            else
               m_Dict.Add( aKey, aItem );
         }
         else
            m_Dict.Add( Guid.NewGuid().ToString(), aItem );
      }

      public override JSONNode Remove( string aKey )
      {
         if( !m_Dict.ContainsKey( aKey ) )
            return null;
         JSONNode tmp = m_Dict[ aKey ];
         m_Dict.Remove( aKey );
         return tmp;
      }

      public override JSONNode Remove( int aIndex )
      {
         if( aIndex < 0 || aIndex >= m_Dict.Count )
            return null;
         var item = m_Dict.ElementAt( aIndex );
         m_Dict.Remove( item.Key );
         return item.Value;
      }

      public override JSONNode Remove( JSONNode aNode )
      {
         try
         {
            var item = m_Dict.Where( k => k.Value == aNode ).First();
            m_Dict.Remove( item.Key );
            return aNode;
         }
         catch
         {
            return null;
         }
      }

      public override IEnumerable<JSONNode> Children
      {
         get
         {
            foreach( KeyValuePair<string, JSONNode> N in m_Dict )
               yield return N.Value;
         }
      }

      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( '{' );
         bool first = true;
         if( inline )
            aMode = JSONTextMode.Compact;
         foreach( var k in m_Dict )
         {
            if( !first )
               aSB.Append( ',' );
            first = false;
            if( aMode == JSONTextMode.Indent )
               aSB.AppendLine();
            if( aMode == JSONTextMode.Indent )
               aSB.Append( ' ', aIndent + aIndentInc );
            aSB.Append( '\"' ).Append( Escape( k.Key ) ).Append( '\"' );
            if( aMode == JSONTextMode.Compact )
               aSB.Append( ':' );
            else
               aSB.Append( " : " );
            k.Value.WriteToStringBuilder( aSB, aIndent + aIndentInc, aIndentInc, aMode );
         }
         if( aMode == JSONTextMode.Indent )
            aSB.AppendLine().Append( ' ', aIndent );
         aSB.Append( '}' );
      }

   }
   // End of JSONObject

   public partial class JSONString : JSONNode
   {
      private string m_Data;

      public override JSONNodeType Tag { get { return JSONNodeType.String; } }
      public override bool IsString { get { return true; } }

      public override Enumerator GetEnumerator() { return new Enumerator(); }


      public override string Value
      {
         get { return m_Data; }
         set
         {
            m_Data = value;
         }
      }

      public JSONString( string aData )
      {
         m_Data = aData;
      }

      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( '\"' ).Append( Escape( m_Data ) ).Append( '\"' );
      }
      public override bool Equals( object obj )
      {
         if( base.Equals( obj ) )
            return true;
         string s = obj as string;
         if( s != null )
            return m_Data == s;
         JSONString s2 = obj as JSONString;
         if( s2 != null )
            return m_Data == s2.m_Data;
         return false;
      }
      public override int GetHashCode()
      {
         return m_Data.GetHashCode();
      }
   }
   // End of JSONString

   public partial class JSONNumber : JSONNode
   {
      private double m_Data;

      public override JSONNodeType Tag { get { return JSONNodeType.Number; } }
      public override bool IsNumber { get { return true; } }
      public override Enumerator GetEnumerator() { return new Enumerator(); }

      public override string Value
      {
         get { return m_Data.ToString(CultureInfo.InvariantCulture); }
         set
         {
            double v;
            if( double.TryParse( value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v ) )
               m_Data = v;
         }
      }

      public override double AsDouble
      {
         get { return m_Data; }
         set { m_Data = value; }
      }

      public JSONNumber( double aData )
      {
         m_Data = aData;
      }

      public JSONNumber( string aData )
      {
         Value = aData;
      }

      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( m_Data );
      }
      private static bool IsNumeric( object value )
      {
         return value is int || value is uint
             || value is float || value is double
             || value is decimal
             || value is long || value is ulong
             || value is short || value is ushort
             || value is sbyte || value is byte;
      }
      public override bool Equals( object obj )
      {
         if( obj == null )
            return false;
         if( base.Equals( obj ) )
            return true;
         JSONNumber s2 = obj as JSONNumber;
         if( s2 != null )
            return m_Data == s2.m_Data;
         if( IsNumeric( obj ) )
            return Convert.ToDouble( obj ) == m_Data;
         return false;
      }
      public override int GetHashCode()
      {
         return m_Data.GetHashCode();
      }
   }
   // End of JSONNumber

   public partial class JSONBool : JSONNode
   {
      private bool m_Data;

      public override JSONNodeType Tag { get { return JSONNodeType.Boolean; } }
      public override bool IsBoolean { get { return true; } }
      public override Enumerator GetEnumerator() { return new Enumerator(); }

      public override string Value
      {
         get { return m_Data.ToString(); }
         set
         {
            bool v;
            if( bool.TryParse( value, out v ) )
               m_Data = v;
         }
      }
      public override bool AsBool
      {
         get { return m_Data; }
         set { m_Data = value; }
      }

      public JSONBool( bool aData )
      {
         m_Data = aData;
      }

      public JSONBool( string aData )
      {
         Value = aData;
      }

      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( ( m_Data ) ? "true" : "false" );
      }
      public override bool Equals( object obj )
      {
         if( obj == null )
            return false;
         if( obj is bool )
            return m_Data == (bool)obj;
         return false;
      }
      public override int GetHashCode()
      {
         return m_Data.GetHashCode();
      }
   }
   // End of JSONBool

   public partial class JSONNull : JSONNode
   {
      static JSONNull m_StaticInstance = new JSONNull();
      public static bool reuseSameInstance = true;
      public static JSONNull CreateOrGet()
      {
         if( reuseSameInstance )
            return m_StaticInstance;
         return new JSONNull();
      }
      private JSONNull() { }

      public override JSONNodeType Tag { get { return JSONNodeType.NullValue; } }
      public override bool IsNull { get { return true; } }
      public override Enumerator GetEnumerator() { return new Enumerator(); }

      public override string Value
      {
         get { return "null"; }
         set { }
      }
      public override bool AsBool
      {
         get { return false; }
         set { }
      }

      public override bool Equals( object obj )
      {
         if( object.ReferenceEquals( this, obj ) )
            return true;
         return ( obj is JSONNull );
      }
      public override int GetHashCode()
      {
         return 0;
      }

      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( "null" );
      }
   }
   // End of JSONNull

   internal partial class JSONLazyCreator : JSONNode
   {
      private JSONNode m_Node = null;
      private string m_Key = null;
      public override JSONNodeType Tag { get { return JSONNodeType.None; } }
      public override Enumerator GetEnumerator() { return new Enumerator(); }

      public JSONLazyCreator( JSONNode aNode )
      {
         m_Node = aNode;
         m_Key = null;
      }

      public JSONLazyCreator( JSONNode aNode, string aKey )
      {
         m_Node = aNode;
         m_Key = aKey;
      }

      private void Set( JSONNode aVal )
      {
         if( m_Key == null )
         {
            m_Node.Add( aVal );
         }
         else
         {
            m_Node.Add( m_Key, aVal );
         }
         m_Node = null; // Be GC friendly.
      }

      public override JSONNode this[ int aIndex ]
      {
         get
         {
            return new JSONLazyCreator( this );
         }
         set
         {
            var tmp = new JSONArray();
            tmp.Add( value );
            Set( tmp );
         }
      }

      public override JSONNode this[ string aKey ]
      {
         get
         {
            return new JSONLazyCreator( this, aKey );
         }
         set
         {
            var tmp = new JSONObject();
            tmp.Add( aKey, value );
            Set( tmp );
         }
      }

      public override void Add( JSONNode aItem )
      {
         var tmp = new JSONArray();
         tmp.Add( aItem );
         Set( tmp );
      }

      public override void Add( string aKey, JSONNode aItem )
      {
         var tmp = new JSONObject();
         tmp.Add( aKey, aItem );
         Set( tmp );
      }

      public static bool operator ==( JSONLazyCreator a, object b )
      {
         if( b == null )
            return true;
         return System.Object.ReferenceEquals( a, b );
      }

      public static bool operator !=( JSONLazyCreator a, object b )
      {
         return !( a == b );
      }

      public override bool Equals( object obj )
      {
         if( obj == null )
            return true;
         return System.Object.ReferenceEquals( this, obj );
      }

      public override int GetHashCode()
      {
         return 0;
      }

      public override int AsInt
      {
         get
         {
            JSONNumber tmp = new JSONNumber( 0 );
            Set( tmp );
            return 0;
         }
         set
         {
            JSONNumber tmp = new JSONNumber( value );
            Set( tmp );
         }
      }

      public override float AsFloat
      {
         get
         {
            JSONNumber tmp = new JSONNumber( 0.0f );
            Set( tmp );
            return 0.0f;
         }
         set
         {
            JSONNumber tmp = new JSONNumber( value );
            Set( tmp );
         }
      }

      public override double AsDouble
      {
         get
         {
            JSONNumber tmp = new JSONNumber( 0.0 );
            Set( tmp );
            return 0.0;
         }
         set
         {
            JSONNumber tmp = new JSONNumber( value );
            Set( tmp );
         }
      }

      public override bool AsBool
      {
         get
         {
            JSONBool tmp = new JSONBool( false );
            Set( tmp );
            return false;
         }
         set
         {
            JSONBool tmp = new JSONBool( value );
            Set( tmp );
         }
      }

      public override JSONArray AsArray
      {
         get
         {
            JSONArray tmp = new JSONArray();
            Set( tmp );
            return tmp;
         }
      }

      public override JSONObject AsObject
      {
         get
         {
            JSONObject tmp = new JSONObject();
            Set( tmp );
            return tmp;
         }
      }
      internal override void WriteToStringBuilder( StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode )
      {
         aSB.Append( "null" );
      }
   }
   // End of JSONLazyCreator

   public static class JSON
   {
      public static JSONNode Parse( string aJSON )
      {
         return JSONNode.Parse( aJSON );
      }
   }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
