using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{/// <summary>
 /// Represents a Windows Script Engine such as JScript, VBScript, etc.
 /// </summary>
   public sealed class ScriptEngine : IDisposable
   {
      /// <summary>
      /// The name of the function used for simple evaluation.
      /// </summary>
      public const string MethodName = "EvalMethod";

      /// <summary>
      /// The default scripting language name.
      /// </summary>
      public const string DefaultLanguage = JavaScriptLanguage;

      /// <summary>
      /// The JavaScript or jscript scripting language name.
      /// </summary>
      public const string JavaScriptLanguage = "javascript";

      /// <summary>
      /// The javascript or jscript scripting language name.
      /// </summary>
      public const string VBScriptLanguage = "vbscript";

      /// <summary>
      /// The chakra javascript engine CLSID. The value is {16d51579-a30b-4c8b-a276-0ff4dc41e755}.
      /// </summary>
      public const string ChakraClsid = "{16d51579-a30b-4c8b-a276-0ff4dc41e755}";

      private IActiveScript _engine;
      private IActiveScriptParse32 _parse32;
      private IActiveScriptParse64 _parse64;
      internal ScriptSite Site;
      private Version _version;
      private string _name;

      [Guid( "BB1A2AE1-A4F9-11cf-8F20-00805F2CD064" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScript
      {
         [PreserveSig]
         int SetScriptSite( IActiveScriptSite pass );
         [PreserveSig]
         int GetScriptSite( Guid riid, out IntPtr site );
         [PreserveSig]
         int SetScriptState( ScriptState state );
         [PreserveSig]
         int GetScriptState( out ScriptState scriptState );
         [PreserveSig]
         int Close();
         [PreserveSig]
         int AddNamedItem( string name, ScriptItem flags );
         [PreserveSig]
         int AddTypeLib( Guid typeLib, uint major, uint minor, uint flags );
         [PreserveSig]
         int GetScriptDispatch( string itemName, out IntPtr dispatch );
         [PreserveSig]
         int GetCurrentScriptThreadID( out uint thread );
         [PreserveSig]
         int GetScriptThreadID( uint win32ThreadId, out uint thread );
         [PreserveSig]
         int GetScriptThreadState( uint thread, out ScriptThreadState state );
         [PreserveSig]
         int InterruptScriptThread( uint thread, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo, uint flags );
         [PreserveSig]
         int Clone( out IActiveScript script );
      }

      [Guid( "4954E0D0-FBC7-11D1-8410-006008C3FBFC" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScriptProperty
      {
         [PreserveSig]
         int GetProperty( int dwProperty, IntPtr pvarIndex, out object pvarValue );
         [PreserveSig]
         int SetProperty( int dwProperty, IntPtr pvarIndex, ref object pvarValue );
      }

      [Guid( "DB01A1E3-A42B-11cf-8F20-00805F2CD064" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScriptSite
      {
         [PreserveSig]
         int GetLCID( out int lcid );
         [PreserveSig]
         int GetItemInfo( string name, ScriptInfo returnMask, out IntPtr item, IntPtr typeInfo );
         [PreserveSig]
         int GetDocVersionString( out string version );
         [PreserveSig]
         int OnScriptTerminate( object result, System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
         [PreserveSig]
         int OnStateChange( ScriptState scriptState );
         [PreserveSig]
         int OnScriptError( IActiveScriptError scriptError );
         [PreserveSig]
         int OnEnterScript();
         [PreserveSig]
         int OnLeaveScript();
      }

      [Guid( "EAE1BA61-A4ED-11cf-8F20-00805F2CD064" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScriptError
      {
         [PreserveSig]
         int GetExceptionInfo( out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
         [PreserveSig]
         int GetSourcePosition( out uint sourceContext, out int lineNumber, out int characterPosition );
         [PreserveSig]
         int GetSourceLineText( out string sourceLine );
      }

      [Guid( "BB1A2AE2-A4F9-11cf-8F20-00805F2CD064" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScriptParse32
      {
         [PreserveSig]
         int InitNew();
         [PreserveSig]
         int AddScriptlet( string defaultName, string code, string itemName, string subItemName, string eventName, string delimiter, IntPtr sourceContextCookie, uint startingLineNumber, ScriptText flags, out string name, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
         [PreserveSig]
         int ParseScriptText( string code, string itemName, IntPtr context, string delimiter, int sourceContextCookie, uint startingLineNumber, ScriptText flags, out object result, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
      }

      [Guid( "C7EF7658-E1EE-480E-97EA-D52CB4D76D17" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
      private interface IActiveScriptParse64
      {
         [PreserveSig]
         int InitNew();
         [PreserveSig]
         int AddScriptlet( string defaultName, string code, string itemName, string subItemName, string eventName, string delimiter, IntPtr sourceContextCookie, uint startingLineNumber, ScriptText flags, out string name, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
         [PreserveSig]
         int ParseScriptText( string code, string itemName, IntPtr context, string delimiter, long sourceContextCookie, uint startingLineNumber, ScriptText flags, out object result, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo );
      }

      [Flags]
      private enum ScriptText
      {
         None = 0,
         //DelayExecution = 1,
         //IsVisible = 2,
         IsExpression = 32,
         IsPersistent = 64,
         //HostManageSource = 128
      }

      [Flags]
      private enum ScriptInfo
      {
         //None = 0,
         //IUnknown = 1,
         ITypeInfo = 2
      }

      [Flags]
      private enum ScriptItem
      {
         //None = 0,
         IsVisible = 2,
         IsSource = 4,
         //GlobalMembers = 8,
         //IsPersistent = 64,
         //CodeOnly = 512,
         //NoCode = 1024
      }

      private enum ScriptThreadState
      {
         //NotInScript = 0,
         //Running = 1
      }

      private enum ScriptState
      {
         Uninitialized = 0,
         Started = 1,
         Connected = 2,
         Disconnected = 3,
         Closed = 4,
         Initialized = 5
      }

      private const int TYPE_E_ELEMENTNOTFOUND = unchecked((int)( 0x8002802B ));
      private const int E_NOTIMPL = -2147467263;

      public static bool TryParseGuid( string s, out Guid guid )
      {
         try
         {
            guid = new Guid( s );
            return true;
         }
         catch
         {
            guid = Guid.Empty;
            return false;
         }
      }

      /// <summary>
      /// Determines if a script engine with the input name exists.
      /// </summary>
      /// <param name="language">The language.</param>
      /// <returns>true if the engine exists; false otherwise.</returns>
      public static Version GetVersion( string language )
      {
         if( language == null )
            throw new ArgumentNullException( "language" );

         Type engine;
         Guid clsid;
         if( TryParseGuid( language, out clsid ) )
         {
            engine = Type.GetTypeFromCLSID( clsid, false );
         }
         else
         {
            engine = Type.GetTypeFromProgID( language, false );
         }
         if( engine == null )
            return null;

         IActiveScript scriptEngine = Activator.CreateInstance( engine ) as IActiveScript;
         if( scriptEngine == null )
            return null;

         IActiveScriptProperty scriptProperty = scriptEngine as IActiveScriptProperty;
         if( scriptProperty == null )
            return new Version( 1, 0, 0, 0 );

         int major = GetProperty( scriptProperty, SCRIPTPROP_MAJORVERSION, 0 );
         int minor = GetProperty( scriptProperty, SCRIPTPROP_MINORVERSION, 0 );
         int revision = GetProperty( scriptProperty, SCRIPTPROP_BUILDNUMBER, 0 );
         Version version = new Version( major, minor, Environment.OSVersion.Version.Build, revision );
         Marshal.ReleaseComObject( scriptProperty );
         Marshal.ReleaseComObject( scriptEngine );
         return version;
      }

      private static T GetProperty<T>( IActiveScriptProperty prop, int index, T defaultValue )
      {
         object value;
         if( prop.GetProperty( index, IntPtr.Zero, out value ) != 0 )
            return defaultValue;

         try
         {
            return (T)Convert.ChangeType( value, typeof( T ) );
         }
         catch
         {
            return defaultValue;
         }
      }

      /// <summary> 
      /// Initializes a new instance of the <see cref="ScriptEngine"/> class. 
      /// </summary> 
      /// <param name="language">The scripting language. Standard Windows Script engines names are 'jscript' or 'vbscript'.</param> 
      public ScriptEngine( string language )
      {
         if( language == null )
            throw new ArgumentNullException( "language" );

         Type engine;
         Guid clsid;
         if( TryParseGuid( language, out clsid ) )
         {
            engine = Type.GetTypeFromCLSID( clsid, true );
         }
         else
         {
            engine = Type.GetTypeFromProgID( language, true );
         }
         _engine = Activator.CreateInstance( engine ) as IActiveScript;
         if( _engine == null )
            throw new ArgumentException( language + " is not an Windows Script Engine", "language" );

         Site = new ScriptSite();
         _engine.SetScriptSite( Site );

         // support 32-bit & 64-bit process 
         if( IntPtr.Size == 4 )
         {
            _parse32 = (IActiveScriptParse32)_engine;
            _parse32.InitNew();
         }
         else
         {
            _parse64 = (IActiveScriptParse64)_engine;
            _parse64.InitNew();
         }
      }

      private const int SCRIPTPROP_NAME = 0x00000000;
      private const int SCRIPTPROP_MAJORVERSION = 0x00000001;
      private const int SCRIPTPROP_MINORVERSION = 0x00000002;
      private const int SCRIPTPROP_BUILDNUMBER = 0x00000003;

      /// <summary>
      /// Gets the engine version.
      /// </summary>
      /// <value>
      /// The version.
      /// </value>
      public Version Version
      {
         get
         {
            if( _version == null )
            {
               int major = GetProperty( SCRIPTPROP_MAJORVERSION, 0 );
               int minor = GetProperty( SCRIPTPROP_MINORVERSION, 0 );
               int revision = GetProperty( SCRIPTPROP_BUILDNUMBER, 0 );
               _version = new Version( major, minor, Environment.OSVersion.Version.Build, revision );
            }
            return _version;
         }
      }

      /// <summary>
      /// Gets the engine name.
      /// </summary>
      /// <value>
      /// The name.
      /// </value>
      public string Name
      {
         get
         {
            if( _name == null )
            {
               _name = GetProperty( SCRIPTPROP_NAME, string.Empty );
            }
            return _name;
         }
      }

      /// <summary>
      /// Gets a script engine property.
      /// </summary>
      /// <typeparam name="T">The expected property type.</typeparam>
      /// <param name="index">The property index.</param>
      /// <param name="defaultValue">The default value if not found.</param>
      /// <returns>The value of the property or the default value.</returns>
      public T GetProperty<T>( int index, T defaultValue )
      {
         object value;
         if( !TryGetProperty( index, out value ) )
            return defaultValue;

         try
         {
            return (T)Convert.ChangeType( value, typeof( T ) );
         }
         catch
         {
            return defaultValue;
         }
      }

      /// <summary>
      /// Gets a script engine property.
      /// </summary>
      /// <param name="index">The property index.</param>
      /// <param name="value">The value.</param>
      /// <returns>true if the property was successfully got; false otherwise.</returns>
      public bool TryGetProperty( int index, out object value )
      {
         value = null;
         IActiveScriptProperty property = _engine as IActiveScriptProperty;
         if( property == null )
            return false;

         return property.GetProperty( index, IntPtr.Zero, out value ) == 0;
      }

      /// <summary>
      /// Sets a script engine property.
      /// </summary>
      /// <param name="index">The property index.</param>
      /// <param name="value">The value.</param>
      /// <returns>true if the property was successfully set; false otherwise.</returns>
      public bool SetProperty( int index, object value )
      {
         IActiveScriptProperty property = _engine as IActiveScriptProperty;
         if( property == null )
            return false;

         return property.SetProperty( index, IntPtr.Zero, ref value ) == 0;
      }

      /// <summary> 
      /// Adds the name of a root-level item to the scripting engine's name space. 
      /// </summary> 
      /// <param name="name">The name. May not be null.</param> 
      /// <param name="value">The value. It must be a ComVisible object.</param> 
      public void SetNamedItem( string name, object value )
      {
         if( name == null )
            throw new ArgumentNullException( "name" );

         _engine.AddNamedItem( name, ScriptItem.IsVisible | ScriptItem.IsSource );
         Site.NamedItems[ name ] = value;
      }

      internal class ScriptSite : IActiveScriptSite
      {
         internal ScriptException LastException;
         internal Dictionary<string, object> NamedItems = new Dictionary<string, object>();

         int IActiveScriptSite.GetLCID( out int lcid )
         {
            lcid = Thread.CurrentThread.CurrentCulture.LCID;
            return 0;
         }

         int IActiveScriptSite.GetItemInfo( string name, ScriptInfo returnMask, out IntPtr item, IntPtr typeInfo )
         {
            item = IntPtr.Zero;
            if( ( returnMask & ScriptInfo.ITypeInfo ) == ScriptInfo.ITypeInfo )
               return E_NOTIMPL;

            object value;
            if( !NamedItems.TryGetValue( name, out value ) )
               return TYPE_E_ELEMENTNOTFOUND;

            item = Marshal.GetIUnknownForObject( value );
            return 0;
         }

         int IActiveScriptSite.GetDocVersionString( out string version )
         {
            version = null;
            return 0;
         }

         int IActiveScriptSite.OnScriptTerminate( object result, System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo )
         {
            return 0;
         }

         int IActiveScriptSite.OnStateChange( ScriptState scriptState )
         {
            return 0;
         }

         int IActiveScriptSite.OnScriptError( IActiveScriptError scriptError )
         {
            string sourceLine = null;
            try
            {
               scriptError.GetSourceLineText( out sourceLine );
            }
            catch
            {
               // happens sometimes... 
            }
            uint sourceContext;
            int lineNumber;
            int characterPosition;
            scriptError.GetSourcePosition( out sourceContext, out lineNumber, out characterPosition );
            lineNumber++;
            characterPosition++;
            System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
            scriptError.GetExceptionInfo( out exceptionInfo );

            string message;
            if( !string.IsNullOrEmpty( sourceLine ) )
            {
               message = "Script exception: {1}. Error number {0} (0x{0:X8}): {2} at line {3}, column {4}. Source line: '{5}'.";
            }
            else
            {
               message = "Script exception: {1}. Error number {0} (0x{0:X8}): {2} at line {3}, column {4}.";
            }
            LastException = new ScriptException( string.Format( message, exceptionInfo.scode, exceptionInfo.bstrSource, exceptionInfo.bstrDescription, lineNumber, characterPosition, sourceLine ) );
            LastException.Column = characterPosition;
            LastException.Description = exceptionInfo.bstrDescription;
            LastException.Line = lineNumber;
            LastException.Number = exceptionInfo.scode;
            LastException.Text = sourceLine;
            return 0;
         }

         int IActiveScriptSite.OnEnterScript()
         {
            LastException = null;
            return 0;
         }

         int IActiveScriptSite.OnLeaveScript()
         {
            return 0;
         }
      }

      /// <summary> 
      /// Evaluates an expression using the specified language. 
      /// </summary> 
      /// <param name="language">The language.</param> 
      /// <param name="expression">The expression. May not be null.</param> 
      /// <returns>The result of the evaluation.</returns> 
      public static object Eval( string language, string expression )
      {
         return Eval( language, expression, null );
      }

      /// <summary> 
      /// Evaluates an expression using the specified language, with an optional array of named items. 
      /// </summary> 
      /// <param name="language">The language.</param> 
      /// <param name="expression">The expression. May not be null.</param> 
      /// <param name="namedItems">The named items array.</param> 
      /// <returns>The result of the evaluation.</returns> 
      public static object Eval( string language, string expression, params KeyValuePair<string, object>[] namedItems )
      {
         if( language == null )
            throw new ArgumentNullException( "language" );

         if( expression == null )
            throw new ArgumentNullException( "expression" );

         using( ScriptEngine engine = new ScriptEngine( language ) )
         {
            if( namedItems != null )
            {
               foreach( KeyValuePair<string, object> kvp in namedItems )
               {
                  engine.SetNamedItem( kvp.Key, kvp.Value );
               }
            }
            return engine.Eval( expression );
         }
      }

      /// <summary> 
      /// Evaluates an expression. 
      /// </summary> 
      /// <param name="expression">The expression. May not be null.</param> 
      /// <returns>The result of the evaluation.</returns> 
      public object Eval( string expression )
      {
         if( expression == null )
            throw new ArgumentNullException( "expression" );

         return Parse( expression, true );
      }

      /// <summary> 
      /// Parses the specified text and returns an object that can be used for evaluation. 
      /// </summary> 
      /// <param name="text">The text to parse.</param> 
      /// <returns>An instance of the ParsedScript class.</returns> 
      public ParsedScript Parse( string text )
      {
         if( text == null )
            throw new ArgumentNullException( "text" );

         return (ParsedScript)Parse( text, false );
      }

      private object Parse( string text, bool expression )
      {
         const string varName = "x___";
         object result;

         _engine.SetScriptState( ScriptState.Connected );

         ScriptText flags = ScriptText.None;
         if( expression )
         {
            flags |= ScriptText.IsExpression;
         }

         try
         {
            // immediate expression computation seems to work only for 64-bit 
            // so hack something for 32-bit... 
            System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
            if( _parse32 != null )
            {
               if( expression )
               {
                  // should work for jscript & vbscript at least... 
                  text = varName + "=" + text;
               }
               _parse32.ParseScriptText( text, null, IntPtr.Zero, null, 0, 0, flags, out result, out exceptionInfo );
            }
            else
            {
               _parse64.ParseScriptText( text, null, IntPtr.Zero, null, 0, 0, flags, out result, out exceptionInfo );
            }
         }
         catch
         {
            if( Site.LastException != null )
               throw Site.LastException;

            throw;
         }

         IntPtr dispatch;
         if( expression )
         {
            // continue  our 32-bit hack... 
            if( _parse32 != null )
            {
               _engine.GetScriptDispatch( null, out dispatch );
               object dp = Marshal.GetObjectForIUnknown( dispatch );
               try
               {
                  return dp.GetType().InvokeMember( varName, BindingFlags.GetProperty, null, dp, null );
               }
               catch
               {
                  if( Site.LastException != null )
                     throw Site.LastException;

                  throw;
               }
            }
            return result;
         }

         _engine.GetScriptDispatch( null, out dispatch );
         ParsedScript parsed = new ParsedScript( this, dispatch );
         return parsed;
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
         if( _parse32 != null )
         {
            Marshal.ReleaseComObject( _parse32 );
            _parse32 = null;
         }

         if( _parse64 != null )
         {
            Marshal.ReleaseComObject( _parse64 );
            _parse64 = null;
         }

         if( _engine != null )
         {
            Marshal.ReleaseComObject( _engine );
            _engine = null;
         }
      }
   }

   public sealed class ParsedScript : IDisposable
   {
      private object _dispatch;
      private readonly ScriptEngine _engine;

      internal ParsedScript( ScriptEngine engine, IntPtr dispatch )
      {
         _engine = engine;
         _dispatch = Marshal.GetObjectForIUnknown( dispatch );
      }

      public object CallMethod( string methodName, params object[] arguments )
      {
         if( _dispatch == null )
            throw new InvalidOperationException();

         if( methodName == null )
            throw new ArgumentNullException( "methodName" );

         try
         {
            return _dispatch.GetType().InvokeMember( methodName, BindingFlags.InvokeMethod, null, _dispatch, arguments );
         }
         catch
         {
            if( _engine.Site.LastException != null )
               throw _engine.Site.LastException;

            throw;
         }
      }

      void IDisposable.Dispose()
      {
         if( _dispatch != null )
         {
            Marshal.ReleaseComObject( _dispatch );
            _dispatch = null;
         }
      }
   }

   [Serializable]
   public class ScriptException : Exception
   {
      public ScriptException()
          : base( "Script Exception" )
      {
      }

      public ScriptException( string message )
          : base( message )
      {
      }

      public ScriptException( Exception innerException )
          : base( null, innerException )
      {
      }

      public ScriptException( string message, Exception innerException )
          : base( message, innerException )
      {
      }

      protected ScriptException( SerializationInfo info, StreamingContext context )
          : base( info, context )
      {
      }

      public string Description { get; internal set; }
      public int Line { get; internal set; }
      public int Column { get; internal set; }
      public int Number { get; internal set; }
      public string Text { get; internal set; }
   }
}
