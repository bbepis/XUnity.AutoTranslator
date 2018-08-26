using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public delegate void MyDownloadStringCompletedEventHandler( object sender, MyDownloadStringCompletedEventArgs e );
   public delegate void MyUploadStringCompletedEventHandler( object sender, MyUploadStringCompletedEventArgs e );

   public class MyDownloadStringCompletedEventArgs : AsyncCompletedEventArgs
   {
      private string result;

      internal MyDownloadStringCompletedEventArgs( string result, Exception error, bool cancelled, object userState ) : base( error, cancelled, userState )
      {
         this.result = result;
      }

      public string Result
      {
         get
         {
            base.RaiseExceptionIfNecessary();
            return this.result;
         }
      }
   }
   public class MyUploadStringCompletedEventArgs : AsyncCompletedEventArgs
   {
      private string result;

      internal MyUploadStringCompletedEventArgs( string result, Exception error, bool cancelled, object userState ) : base( error, cancelled, userState )
      {
         this.result = result;
      }

      public string Result
      {
         get
         {
            base.RaiseExceptionIfNecessary();
            return this.result;
         }
      }
   }

   public class MyAsyncCompletedEventArgs : EventArgs
   {
      private bool _cancelled;
      private Exception _error;
      private object _userState;

      public MyAsyncCompletedEventArgs( Exception error, bool cancelled, object userState )
      {
         this._error = error;
         this._cancelled = cancelled;
         this._userState = userState;
      }

      protected void RaiseExceptionIfNecessary()
      {
         if( this._error != null )
         {
            throw new TargetInvocationException( this._error );
         }
         if( this._cancelled )
         {
            throw new InvalidOperationException( "The operation was cancelled" );
         }
      }

      public bool Cancelled
      {
         get
         {
            return this._cancelled;
         }
      }

      public Exception Error
      {
         get
         {
            return this._error;
         }
      }

      public object UserState
      {
         get
         {
            return this._userState;
         }
      }
   }

   public class MyWebClient
   {
      public static readonly string ConnectionGroupName = Guid.NewGuid().ToString();

      private bool async;
      private Uri baseAddress;
      private string baseString;
      private ICredentials credentials;
      private System.Text.Encoding encoding = System.Text.Encoding.Default;
      private WebHeaderCollection headers;
      private static byte[] hexBytes = new byte[ 0x10 ];
      private bool is_busy;
      private IWebProxy proxy;
      private NameValueCollection queryString;
      private WebHeaderCollection responseHeaders;
      private static readonly string urlEncodedCType = "application/x-www-form-urlencoded";

      public event MyDownloadStringCompletedEventHandler DownloadStringCompleted;
      public event MyUploadStringCompletedEventHandler UploadStringCompleted;

      static MyWebClient()
      {
         int index = 0;
         int num2 = 0x30;
         while( num2 <= 0x39 )
         {
            hexBytes[ index ] = (byte)num2;
            num2++;
            index++;
         }
         int num3 = 0x61;
         while( num3 <= 0x66 )
         {
            hexBytes[ index ] = (byte)num3;
            num3++;
            index++;
         }
      }

      //public void CancelAsync()
      //{
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      if( this.async_thread != null )
      //      {
      //         Thread thread = this.async_thread;
      //         this.CompleteAsync();
      //         thread.Interrupt();
      //      }
      //   }
      //}

      private void CheckBusy()
      {
         if( this.IsBusy )
         {
            throw new NotSupportedException( "WebClient does not support conccurent I/O operations." );
         }
      }

      private void CompleteAsync()
      {
         MyWebClient client = this;
         lock( client )
         {
            this.is_busy = false;
         }
      }

      private Uri CreateUri( string address )
      {
         return this.MakeUri( address );
      }

      private Uri CreateUri( Uri address )
      {
         string query = address.Query;
         if( string.IsNullOrEmpty( query ) )
         {
            query = this.GetQueryString( true );
         }
         if( ( this.baseAddress == null ) && ( query == null ) )
         {
            return address;
         }
         if( this.baseAddress == null )
         {
            return new Uri( address.ToString() + query, query != null );
         }
         if( query == null )
         {
            return new Uri( this.baseAddress, address.ToString() );
         }
         return new Uri( this.baseAddress, address.ToString() + query, query != null );
      }

      private string DetermineMethod( Uri address, string method, bool is_upload )
      {
         if( method != null )
         {
            return method;
         }
         if( address.Scheme == Uri.UriSchemeFtp )
         {
            return ( !is_upload ? "RETR" : "STOR" );
         }
         return ( !is_upload ? "GET" : "POST" );
      }

      public byte[] DownloadData( string address )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.DownloadData( this.CreateUri( address ) );
      }

      public byte[] DownloadData( Uri address )
      {
         byte[] buffer;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         try
         {
            this.SetBusy();
            this.async = false;
            buffer = this.DownloadDataCore( address, null );
         }
         finally
         {
            this.is_busy = false;
         }
         return buffer;
      }

      //public void DownloadDataAsync( Uri address )
      //{
      //   this.DownloadDataAsync( address, null );
      //}

      //public void DownloadDataAsync( Uri address, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         try
      //         {
      //            byte[] result = this.DownloadDataCore( (Uri)objArray[ 0 ], objArray[ 1 ] );
      //            this.OnDownloadDataCompleted( new DownloadDataCompletedEventArgs( result, null, false, objArray[ 1 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            this.OnDownloadDataCompleted( new DownloadDataCompletedEventArgs( null, null, true, objArray[ 1 ] ) );
      //            throw;
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnDownloadDataCompleted( new DownloadDataCompletedEventArgs( null, exception, false, objArray[ 1 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      private byte[] DownloadDataCore( Uri address, object userToken )
      {
         WebRequest request = null;
         byte[] buffer;
         try
         {
            request = this.SetupRequest( address );
            WebResponse webResponse = this.GetWebResponse( request );
            Stream responseStream = webResponse.GetResponseStream();
            buffer = this.ReadAll( responseStream, (int)webResponse.ContentLength, userToken );
            responseStream.Close();
            webResponse.Close();
         }
         catch( ThreadInterruptedException )
         {
            if( request != null )
            {
               request.Abort();
            }
            throw;
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception2 )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception2 );
         }
         return buffer;
      }

      //public void DownloadFile( string address, string fileName )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   this.DownloadFile( this.CreateUri( address ), fileName );
      //}

      //public void DownloadFile( Uri address, string fileName )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   if( fileName == null )
      //   {
      //      throw new ArgumentNullException( "fileName" );
      //   }
      //   try
      //   {
      //      this.SetBusy();
      //      this.async = false;
      //      this.DownloadFileCore( address, fileName, null );
      //   }
      //   catch( WebException )
      //   {
      //      throw;
      //   }
      //   catch( Exception exception2 )
      //   {
      //      throw new WebException( "An error occurred performing a WebClient request.", exception2 );
      //   }
      //   finally
      //   {
      //      this.is_busy = false;
      //   }
      //}

      //public void DownloadFileAsync( Uri address, string fileName )
      //{
      //   this.DownloadFileAsync( address, fileName, null );
      //}

      //public void DownloadFileAsync( Uri address, string fileName, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   if( fileName == null )
      //   {
      //      throw new ArgumentNullException( "fileName" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, fileName, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         try
      //         {
      //            this.DownloadFileCore( (Uri)objArray[ 0 ], (string)objArray[ 1 ], objArray[ 2 ] );
      //            this.OnDownloadFileCompleted( new AsyncCompletedEventArgs( null, false, objArray[ 2 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            this.OnDownloadFileCompleted( new AsyncCompletedEventArgs( null, true, objArray[ 2 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnDownloadFileCompleted( new AsyncCompletedEventArgs( exception, false, objArray[ 2 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      //private void DownloadFileCore( Uri address, string fileName, object userToken )
      //{
      //   WebRequest request = null;
      //   FileStream stream = new FileStream( fileName, FileMode.Create );
      //   try
      //   {
      //      request = this.SetupRequest( address );
      //      WebResponse webResponse = this.GetWebResponse( request );
      //      Stream responseStream = webResponse.GetResponseStream();
      //      int contentLength = (int)webResponse.ContentLength;
      //      int count = ( ( contentLength > -1 ) && ( contentLength <= 0x8000 ) ) ? contentLength : 0x8000;
      //      byte[] buffer = new byte[ count ];
      //      int num3 = 0;
      //      long bytesReceived = 0L;
      //      while( ( num3 = responseStream.Read( buffer, 0, count ) ) != 0 )
      //      {
      //         if( this.async )
      //         {
      //            bytesReceived += num3;
      //            this.OnDownloadProgressChanged( new DownloadProgressChangedEventArgs( bytesReceived, webResponse.ContentLength, userToken ) );
      //         }
      //         stream.Write( buffer, 0, num3 );
      //      }
      //   }
      //   catch( ThreadInterruptedException )
      //   {
      //      if( request != null )
      //      {
      //         request.Abort();
      //      }
      //      throw;
      //   }
      //   finally
      //   {
      //      if( stream != null )
      //      {
      //         stream.Dispose();
      //      }
      //   }
      //}

      public string DownloadString( string address )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.encoding.GetString( this.DownloadData( this.CreateUri( address ) ) );
      }

      public string DownloadString( Uri address )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.encoding.GetString( this.DownloadData( this.CreateUri( address ) ) );
      }

      public void DownloadStringAsync( Uri address )
      {
         this.DownloadStringAsync( address, null );
      }

      public void DownloadStringAsync( Uri address, object userToken )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         MyWebClient client = this;
         lock( client )
         {
            this.SetBusy();
            this.async = true;
            object[] parameter = new object[] { address, userToken };
            ThreadPool.QueueUserWorkItem( delegate ( object state )
            {
               object[] objArray = (object[])state;
               try
               {
                  string result = this.encoding.GetString( this.DownloadDataCore( (Uri)objArray[ 0 ], objArray[ 1 ] ) );
                  this.OnDownloadStringCompleted( new MyDownloadStringCompletedEventArgs( result, null, false, objArray[ 1 ] ) );
               }
               catch( ThreadInterruptedException )
               {
                  this.OnDownloadStringCompleted( new MyDownloadStringCompletedEventArgs( null, null, true, objArray[ 1 ] ) );
               }
               catch( Exception exception )
               {
                  this.OnDownloadStringCompleted( new MyDownloadStringCompletedEventArgs( null, exception, false, objArray[ 1 ] ) );
               }
            }, parameter );
         }
      }

      private static Exception GetMustImplement()
      {
         return new NotImplementedException();
      }

      private string GetQueryString( bool add_qmark )
      {
         if( ( this.queryString == null ) || ( this.queryString.Count == 0 ) )
         {
            return null;
         }
         StringBuilder builder = new StringBuilder();
         if( add_qmark )
         {
            builder.Append( '?' );
         }
         IEnumerator enumerator = this.queryString.GetEnumerator();
         try
         {
            while( enumerator.MoveNext() )
            {
               string current = (string)enumerator.Current;
               builder.AppendFormat( "{0}={1}&", current, this.UrlEncode( this.queryString[ current ] ) );
            }
         }
         finally
         {
            IDisposable disposable = enumerator as IDisposable;
            if( disposable != null )
            {
               disposable.Dispose();
            }
         }
         if( builder.Length != 0 )
         {
            builder.Length--;
         }
         if( builder.Length == 0 )
         {
            return null;
         }
         return builder.ToString();
      }

      protected virtual WebRequest GetWebRequest( Uri address )
      {
         return WebRequest.Create( address );
      }

      protected virtual WebResponse GetWebResponse( WebRequest request )
      {
         WebResponse response = request.GetResponse();
         this.responseHeaders = response.Headers;
         return response;
      }

      protected virtual WebResponse GetWebResponse( WebRequest request, IAsyncResult result )
      {
         WebResponse response = request.EndGetResponse( result );
         this.responseHeaders = response.Headers;
         return response;
      }

      private Uri MakeUri( string path )
      {
         string queryString = this.GetQueryString( true );
         if( ( this.baseAddress == null ) && ( queryString == null ) )
         {
            try
            {
               return new Uri( path );
            }
            catch( ArgumentNullException )
            {
               path = Path.GetFullPath( path );
               return new Uri( "file://" + path );
            }
            catch( UriFormatException )
            {
               path = Path.GetFullPath( path );
               return new Uri( "file://" + path );
            }
         }
         if( this.baseAddress == null )
         {
            return new Uri( path + queryString, queryString != null );
         }
         if( queryString == null )
         {
            return new Uri( this.baseAddress, path );
         }
         return new Uri( this.baseAddress, path + queryString, queryString != null );
      }

      //protected virtual void OnDownloadDataCompleted( DownloadDataCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.DownloadDataCompleted != null )
      //   {
      //      this.DownloadDataCompleted( this, args );
      //   }
      //}

      //protected virtual void OnDownloadFileCompleted( AsyncCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.DownloadFileCompleted != null )
      //   {
      //      this.DownloadFileCompleted( this, args );
      //   }
      //}

      //protected virtual void OnDownloadProgressChanged( DownloadProgressChangedEventArgs e )
      //{
      //   if( this.DownloadProgressChanged != null )
      //   {
      //      this.DownloadProgressChanged( this, e );
      //   }
      //}

      protected virtual void OnDownloadStringCompleted( MyDownloadStringCompletedEventArgs args )
      {
         this.CompleteAsync();
         if( this.DownloadStringCompleted != null )
         {
            this.DownloadStringCompleted( this, args );
         }
      }

      //protected virtual void OnOpenReadCompleted( OpenReadCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.OpenReadCompleted != null )
      //   {
      //      this.OpenReadCompleted( this, args );
      //   }
      //}

      //protected virtual void OnOpenWriteCompleted( OpenWriteCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.OpenWriteCompleted != null )
      //   {
      //      this.OpenWriteCompleted( this, args );
      //   }
      //}

      //protected virtual void OnUploadDataCompleted( UploadDataCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.UploadDataCompleted != null )
      //   {
      //      this.UploadDataCompleted( this, args );
      //   }
      //}

      //protected virtual void OnUploadFileCompleted( UploadFileCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.UploadFileCompleted != null )
      //   {
      //      this.UploadFileCompleted( this, args );
      //   }
      //}

      //protected virtual void OnUploadProgressChanged( UploadProgressChangedEventArgs e )
      //{
      //   if( this.UploadProgressChanged != null )
      //   {
      //      this.UploadProgressChanged( this, e );
      //   }
      //}

      protected virtual void OnUploadStringCompleted( MyUploadStringCompletedEventArgs args )
      {
         this.CompleteAsync();
         if( this.UploadStringCompleted != null )
         {
            this.UploadStringCompleted( this, args );
         }
      }

      //protected virtual void OnUploadValuesCompleted( UploadValuesCompletedEventArgs args )
      //{
      //   this.CompleteAsync();
      //   if( this.UploadValuesCompleted != null )
      //   {
      //      this.UploadValuesCompleted( this, args );
      //   }
      //}

      public Stream OpenRead( string address )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.OpenRead( this.CreateUri( address ) );
      }

      public Stream OpenRead( Uri address )
      {
         Stream responseStream;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         WebRequest request = null;
         try
         {
            this.SetBusy();
            this.async = false;
            request = this.SetupRequest( address );
            responseStream = this.GetWebResponse( request ).GetResponseStream();
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception2 )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception2 );
         }
         finally
         {
            this.is_busy = false;
         }
         return responseStream;
      }

      //public void OpenReadAsync( Uri address )
      //{
      //   this.OpenReadAsync( address, null );
      //}

      //public void OpenReadAsync( Uri address, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         WebRequest request = null;
      //         try
      //         {
      //            request = this.SetupRequest( (Uri)objArray[ 0 ] );
      //            Stream result = this.GetWebResponse( request ).GetResponseStream();
      //            this.OnOpenReadCompleted( new OpenReadCompletedEventArgs( result, null, false, objArray[ 1 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            if( request != null )
      //            {
      //               request.Abort();
      //            }
      //            this.OnOpenReadCompleted( new OpenReadCompletedEventArgs( null, null, true, objArray[ 1 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnOpenReadCompleted( new OpenReadCompletedEventArgs( null, exception, false, objArray[ 1 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      public Stream OpenWrite( string address )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.OpenWrite( this.CreateUri( address ) );
      }

      public Stream OpenWrite( Uri address )
      {
         return this.OpenWrite( address, null );
      }

      public Stream OpenWrite( string address, string method )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.OpenWrite( this.CreateUri( address ), method );
      }

      public Stream OpenWrite( Uri address, string method )
      {
         Stream requestStream;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         try
         {
            this.SetBusy();
            this.async = false;
            requestStream = this.SetupRequest( address, method, true ).GetRequestStream();
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception2 )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception2 );
         }
         finally
         {
            this.is_busy = false;
         }
         return requestStream;
      }

      //public void OpenWriteAsync( Uri address )
      //{
      //   this.OpenWriteAsync( address, null );
      //}

      //public void OpenWriteAsync( Uri address, string method )
      //{
      //   this.OpenWriteAsync( address, method, null );
      //}

      //public void OpenWriteAsync( Uri address, string method, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, method, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         WebRequest request = null;
      //         try
      //         {
      //            request = this.SetupRequest( (Uri)objArray[ 0 ], (string)objArray[ 1 ], true );
      //            Stream result = request.GetRequestStream();
      //            this.OnOpenWriteCompleted( new OpenWriteCompletedEventArgs( result, null, false, objArray[ 2 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            if( request != null )
      //            {
      //               request.Abort();
      //            }
      //            this.OnOpenWriteCompleted( new OpenWriteCompletedEventArgs( null, null, true, objArray[ 2 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnOpenWriteCompleted( new OpenWriteCompletedEventArgs( null, exception, false, objArray[ 2 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      private byte[] ReadAll( Stream stream, int length, object userToken )
      {
         MemoryStream stream2 = null;
         bool flag = length == -1;
         int count = !flag ? length : 0x2000;
         if( flag )
         {
            stream2 = new MemoryStream();
         }
         int num2 = 0;
         int offset = 0;
         byte[] buffer = new byte[ count ];
         while( ( num2 = stream.Read( buffer, offset, count ) ) != 0 )
         {
            if( flag )
            {
               stream2.Write( buffer, 0, num2 );
            }
            else
            {
               offset += num2;
               count -= num2;
            }
         }
         if( flag )
         {
            return stream2.ToArray();
         }
         return buffer;
      }

      private void SetBusy()
      {
         MyWebClient client = this;
         lock( client )
         {
            this.CheckBusy();
            this.is_busy = true;
         }
      }

      private WebRequest SetupRequest( Uri uri )
      {
         WebRequest webRequest = this.GetWebRequest( uri );
         webRequest.ConnectionGroupName = ConnectionGroupName;
         if( this.Proxy != null )
         {
            webRequest.Proxy = this.Proxy;
         }
         webRequest.Credentials = this.credentials;
         if( ( ( this.headers != null ) && ( this.headers.Count != 0 ) ) && ( webRequest is HttpWebRequest ) )
         {
            HttpWebRequest request2 = (HttpWebRequest)webRequest;
            string str = this.headers[ "Expect" ];
            string str2 = this.headers[ "Content-Type" ];
            string str3 = this.headers[ "Accept" ];
            string str4 = this.headers[ "Connection" ];
            string str5 = this.headers[ "User-Agent" ];
            string str6 = this.headers[ "Referer" ];
            this.headers.Remove( "Expect" );
            this.headers.Remove( "Content-Type" );
            this.headers.Remove( "Accept" );
            this.headers.Remove( "Connection" );
            this.headers.Remove( "Referer" );
            this.headers.Remove( "User-Agent" );
            webRequest.Headers = this.headers;
            if( ( str != null ) && ( str.Length > 0 ) )
            {
               request2.Expect = str;
            }
            if( ( str3 != null ) && ( str3.Length > 0 ) )
            {
               request2.Accept = str3;
            }
            if( ( str2 != null ) && ( str2.Length > 0 ) )
            {
               request2.ContentType = str2;
            }
            if( ( str4 != null ) && ( str4.Length > 0 ) )
            {
               request2.Connection = str4;
            }
            if( ( str5 != null ) && ( str5.Length > 0 ) )
            {
               request2.UserAgent = str5;
            }
            if( ( str6 != null ) && ( str6.Length > 0 ) )
            {
               request2.Referer = str6;
            }
         }
         this.responseHeaders = null;
         return webRequest;
      }

      private WebRequest SetupRequest( Uri uri, string method, bool is_upload )
      {
         WebRequest request = this.SetupRequest( uri );
         request.Method = this.DetermineMethod( uri, method, is_upload );
         return request;
      }

      public byte[] UploadData( string address, byte[] data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.UploadData( this.CreateUri( address ), data );
      }

      public byte[] UploadData( Uri address, byte[] data )
      {
         return this.UploadData( address, null, data );
      }

      public byte[] UploadData( string address, string method, byte[] data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.UploadData( this.CreateUri( address ), method, data );
      }

      public byte[] UploadData( Uri address, string method, byte[] data )
      {
         byte[] buffer;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         try
         {
            this.SetBusy();
            this.async = false;
            buffer = this.UploadDataCore( address, method, data, null );
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception );
         }
         finally
         {
            this.is_busy = false;
         }
         return buffer;
      }

      //public void UploadDataAsync( Uri address, byte[] data )
      //{
      //   this.UploadDataAsync( address, null, data );
      //}

      //public void UploadDataAsync( Uri address, string method, byte[] data )
      //{
      //   this.UploadDataAsync( address, method, data, null );
      //}

      //public void UploadDataAsync( Uri address, string method, byte[] data, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   if( data == null )
      //   {
      //      throw new ArgumentNullException( "data" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, method, data, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         try
      //         {
      //            byte[] result = this.UploadDataCore( (Uri)objArray[ 0 ], (string)objArray[ 1 ], (byte[])objArray[ 2 ], objArray[ 3 ] );
      //            this.OnUploadDataCompleted( new UploadDataCompletedEventArgs( result, null, false, objArray[ 3 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            this.OnUploadDataCompleted( new UploadDataCompletedEventArgs( null, null, true, objArray[ 3 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnUploadDataCompleted( new UploadDataCompletedEventArgs( null, exception, false, objArray[ 3 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      private byte[] UploadDataCore( Uri address, string method, byte[] data, object userToken )
      {
         byte[] buffer;
         WebRequest request = this.SetupRequest( address, method, true );
         try
         {
            int length = data.Length;
            request.ContentLength = length;
            using( Stream stream = request.GetRequestStream() )
            {
               stream.Write( data, 0, length );
            }
            WebResponse webResponse = this.GetWebResponse( request );
            Stream responseStream = webResponse.GetResponseStream();
            buffer = this.ReadAll( responseStream, (int)webResponse.ContentLength, userToken );
         }
         catch( ThreadInterruptedException )
         {
            if( request != null )
            {
               request.Abort();
            }
            throw;
         }
         return buffer;
      }

      public byte[] UploadFile( string address, string fileName )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.UploadFile( this.CreateUri( address ), fileName );
      }

      public byte[] UploadFile( Uri address, string fileName )
      {
         return this.UploadFile( address, null, fileName );
      }

      public byte[] UploadFile( string address, string method, string fileName )
      {
         return this.UploadFile( this.CreateUri( address ), method, fileName );
      }

      public byte[] UploadFile( Uri address, string method, string fileName )
      {
         byte[] buffer;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( fileName == null )
         {
            throw new ArgumentNullException( "fileName" );
         }
         try
         {
            this.SetBusy();
            this.async = false;
            buffer = this.UploadFileCore( address, method, fileName, null );
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception2 )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception2 );
         }
         finally
         {
            this.is_busy = false;
         }
         return buffer;
      }

      //public void UploadFileAsync( Uri address, string fileName )
      //{
      //   this.UploadFileAsync( address, null, fileName );
      //}

      //public void UploadFileAsync( Uri address, string method, string fileName )
      //{
      //   this.UploadFileAsync( address, method, fileName, null );
      //}

      //public void UploadFileAsync( Uri address, string method, string fileName, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   if( fileName == null )
      //   {
      //      throw new ArgumentNullException( "fileName" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.SetBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, method, fileName, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         try
      //         {
      //            byte[] result = this.UploadFileCore( (Uri)objArray[ 0 ], (string)objArray[ 1 ], (string)objArray[ 2 ], objArray[ 3 ] );
      //            this.OnUploadFileCompleted( new UploadFileCompletedEventArgs( result, null, false, objArray[ 3 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            this.OnUploadFileCompleted( new UploadFileCompletedEventArgs( null, null, true, objArray[ 3 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnUploadFileCompleted( new UploadFileCompletedEventArgs( null, exception, false, objArray[ 3 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      private byte[] UploadFileCore( Uri address, string method, string fileName, object userToken )
      {
         string str = this.Headers[ "Content-Type" ];
         if( str != null )
         {
            if( str.ToLower().StartsWith( "multipart/" ) )
            {
               throw new WebException( "Content-Type cannot be set to a multipart type for this request." );
            }
         }
         else
         {
            str = "application/octet-stream";
         }
         string str3 = "------------" + DateTime.Now.Ticks.ToString( "x" );
         this.Headers[ "Content-Type" ] = string.Format( "multipart/form-data; boundary={0}", str3 );
         Stream requestStream = null;
         Stream stream2 = null;
         byte[] buffer = null;
         fileName = Path.GetFullPath( fileName );
         WebRequest request = null;
         try
         {
            int num;
            stream2 = System.IO.File.OpenRead( fileName );
            request = this.SetupRequest( address, method, true );
            requestStream = request.GetRequestStream();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes( "--" + str3 + "\r\n" );
            requestStream.Write( bytes, 0, bytes.Length );
            string s = string.Format( "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", Path.GetFileName( fileName ), str );
            byte[] buffer3 = System.Text.Encoding.UTF8.GetBytes( s );
            requestStream.Write( buffer3, 0, buffer3.Length );
            byte[] buffer4 = new byte[ 0x1000 ];
            while( ( num = stream2.Read( buffer4, 0, 0x1000 ) ) != 0 )
            {
               requestStream.Write( buffer4, 0, num );
            }
            requestStream.WriteByte( 13 );
            requestStream.WriteByte( 10 );
            requestStream.Write( bytes, 0, bytes.Length );
            requestStream.Close();
            requestStream = null;
            WebResponse webResponse = this.GetWebResponse( request );
            Stream responseStream = webResponse.GetResponseStream();
            buffer = this.ReadAll( responseStream, (int)webResponse.ContentLength, userToken );
         }
         catch( ThreadInterruptedException )
         {
            if( request != null )
            {
               request.Abort();
            }
            throw;
         }
         finally
         {
            if( stream2 != null )
            {
               stream2.Close();
            }
            if( requestStream != null )
            {
               requestStream.Close();
            }
         }
         return buffer;
      }

      public string UploadString( string address, string data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         byte[] bytes = this.UploadData( address, this.encoding.GetBytes( data ) );
         return this.encoding.GetString( bytes );
      }

      public string UploadString( Uri address, string data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         byte[] bytes = this.UploadData( address, this.encoding.GetBytes( data ) );
         return this.encoding.GetString( bytes );
      }

      public string UploadString( string address, string method, string data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         byte[] bytes = this.UploadData( address, method, this.encoding.GetBytes( data ) );
         return this.encoding.GetString( bytes );
      }

      public string UploadString( Uri address, string method, string data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         byte[] bytes = this.UploadData( address, method, this.encoding.GetBytes( data ) );
         return this.encoding.GetString( bytes );
      }

      public void UploadStringAsync( Uri address, string data )
      {
         this.UploadStringAsync( address, null, data );
      }

      public void UploadStringAsync( Uri address, string method, string data )
      {
         this.UploadStringAsync( address, method, data, null );
      }

      public void UploadStringAsync( Uri address, string method, string data, object userToken )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         MyWebClient client = this;
         lock( client )
         {
            this.CheckBusy();
            this.async = true;
            object[] parameter = new object[] { address, method, data, userToken };
            ThreadPool.QueueUserWorkItem( delegate ( object state )
            {
               object[] objArray = (object[])state;
               try
               {
                  string result = this.UploadString( (Uri)objArray[ 0 ], (string)objArray[ 1 ], (string)objArray[ 2 ] );
                  this.OnUploadStringCompleted( new MyUploadStringCompletedEventArgs( result, null, false, objArray[ 3 ] ) );
               }
               catch( ThreadInterruptedException )
               {
                  this.OnUploadStringCompleted( new MyUploadStringCompletedEventArgs( null, null, true, objArray[ 3 ] ) );
               }
               catch( Exception exception )
               {
                  this.OnUploadStringCompleted( new MyUploadStringCompletedEventArgs( null, exception, false, objArray[ 3 ] ) );
               }
            }, parameter );
         }
      }

      public byte[] UploadValues( string address, NameValueCollection data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.UploadValues( this.CreateUri( address ), data );
      }

      public byte[] UploadValues( Uri address, NameValueCollection data )
      {
         return this.UploadValues( address, null, data );
      }

      public byte[] UploadValues( string address, string method, NameValueCollection data )
      {
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         return this.UploadValues( this.CreateUri( address ), method, data );
      }

      public byte[] UploadValues( Uri address, string method, NameValueCollection data )
      {
         byte[] buffer;
         if( address == null )
         {
            throw new ArgumentNullException( "address" );
         }
         if( data == null )
         {
            throw new ArgumentNullException( "data" );
         }
         try
         {
            this.SetBusy();
            this.async = false;
            buffer = this.UploadValuesCore( address, method, data, null );
         }
         catch( WebException )
         {
            throw;
         }
         catch( Exception exception2 )
         {
            throw new WebException( "An error occurred performing a WebClient request.", exception2 );
         }
         finally
         {
            this.is_busy = false;
         }
         return buffer;
      }

      //public void UploadValuesAsync( Uri address, NameValueCollection values )
      //{
      //   this.UploadValuesAsync( address, null, values );
      //}

      //public void UploadValuesAsync( Uri address, string method, NameValueCollection values )
      //{
      //   this.UploadValuesAsync( address, method, values, null );
      //}

      //public void UploadValuesAsync( Uri address, string method, NameValueCollection values, object userToken )
      //{
      //   if( address == null )
      //   {
      //      throw new ArgumentNullException( "address" );
      //   }
      //   if( values == null )
      //   {
      //      throw new ArgumentNullException( "values" );
      //   }
      //   MyWebClient client = this;
      //   lock( client )
      //   {
      //      this.CheckBusy();
      //      this.async = true;
      //      object[] parameter = new object[] { address, method, values, userToken };
      //      ThreadPool.QueueUserWorkItem( delegate ( object state )
      //      {
      //         object[] objArray = (object[])state;
      //         try
      //         {
      //            byte[] result = this.UploadValuesCore( (Uri)objArray[ 0 ], (string)objArray[ 1 ], (NameValueCollection)objArray[ 2 ], objArray[ 3 ] );
      //            this.OnUploadValuesCompleted( new UploadValuesCompletedEventArgs( result, null, false, objArray[ 3 ] ) );
      //         }
      //         catch( ThreadInterruptedException )
      //         {
      //            this.OnUploadValuesCompleted( new UploadValuesCompletedEventArgs( null, null, true, objArray[ 3 ] ) );
      //         }
      //         catch( Exception exception )
      //         {
      //            this.OnUploadValuesCompleted( new UploadValuesCompletedEventArgs( null, exception, false, objArray[ 3 ] ) );
      //         }
      //      }, parameter );
      //   }
      //}

      private byte[] UploadValuesCore( Uri uri, string method, NameValueCollection data, object userToken )
      {
         byte[] buffer3;
         string strA = this.Headers[ "Content-Type" ];
         if( ( strA != null ) && ( string.Compare( strA, urlEncodedCType, true ) != 0 ) )
         {
            throw new WebException( "Content-Type header cannot be changed from its default value for this request." );
         }
         this.Headers[ "Content-Type" ] = urlEncodedCType;
         WebRequest request = this.SetupRequest( uri, method, true );
         try
         {
            MemoryStream stream = new MemoryStream();
            IEnumerator enumerator = data.GetEnumerator();
            try
            {
               while( enumerator.MoveNext() )
               {
                  string current = (string)enumerator.Current;
                  byte[] bytes = System.Text.Encoding.UTF8.GetBytes( current );
                  UrlEncodeAndWrite( stream, bytes );
                  stream.WriteByte( 0x3d );
                  bytes = System.Text.Encoding.UTF8.GetBytes( data[ current ] );
                  UrlEncodeAndWrite( stream, bytes );
                  stream.WriteByte( 0x26 );
               }
            }
            finally
            {
               IDisposable disposable = enumerator as IDisposable;
               if( disposable != null )
               {
                  disposable.Dispose();
               }
            }
            int length = (int)stream.Length;
            if( length > 0 )
            {
               stream.SetLength( (long)( --length ) );
            }
            byte[] buffer2 = stream.GetBuffer();
            request.ContentLength = length;
            using( Stream stream2 = request.GetRequestStream() )
            {
               stream2.Write( buffer2, 0, length );
            }
            stream.Close();
            WebResponse webResponse = this.GetWebResponse( request );
            Stream responseStream = webResponse.GetResponseStream();
            buffer3 = this.ReadAll( responseStream, (int)webResponse.ContentLength, userToken );
         }
         catch( ThreadInterruptedException )
         {
            request.Abort();
            throw;
         }
         return buffer3;
      }

      private string UrlEncode( string str )
      {
         StringBuilder builder = new StringBuilder();
         int length = str.Length;
         for( int i = 0 ; i < length ; i++ )
         {
            char ch = str[ i ];
            if( ch == ' ' )
            {
               builder.Append( '+' );
            }
            else if( ( ( ( ( ch < '0' ) && ( ch != '-' ) ) && ( ch != '.' ) ) || ( ( ch < 'A' ) && ( ch > '9' ) ) ) || ( ( ( ( ch > 'Z' ) && ( ch < 'a' ) ) && ( ch != '_' ) ) || ( ch > 'z' ) ) )
            {
               builder.Append( '%' );
               int index = ch >> 4;
               builder.Append( (char)hexBytes[ index ] );
               index = ch & '\x000f';
               builder.Append( (char)hexBytes[ index ] );
            }
            else
            {
               builder.Append( ch );
            }
         }
         return builder.ToString();
      }

      private static void UrlEncodeAndWrite( Stream stream, byte[] bytes )
      {
         if( bytes != null )
         {
            int length = bytes.Length;
            if( length != 0 )
            {
               for( int i = 0 ; i < length ; i++ )
               {
                  char ch = (char)bytes[ i ];
                  if( ch == ' ' )
                  {
                     stream.WriteByte( 0x2b );
                  }
                  else if( ( ( ( ( ch < '0' ) && ( ch != '-' ) ) && ( ch != '.' ) ) || ( ( ch < 'A' ) && ( ch > '9' ) ) ) || ( ( ( ( ch > 'Z' ) && ( ch < 'a' ) ) && ( ch != '_' ) ) || ( ch > 'z' ) ) )
                  {
                     stream.WriteByte( 0x25 );
                     int index = ch >> 4;
                     stream.WriteByte( hexBytes[ index ] );
                     index = ch & '\x000f';
                     stream.WriteByte( hexBytes[ index ] );
                  }
                  else
                  {
                     stream.WriteByte( (byte)ch );
                  }
               }
            }
         }
      }

      public string BaseAddress
      {
         get
         {
            if( ( this.baseString == null ) && ( this.baseAddress == null ) )
            {
               return string.Empty;
            }
            this.baseString = this.baseAddress.ToString();
            return this.baseString;
         }
         set
         {
            if( ( value == null ) || ( value.Length == 0 ) )
            {
               this.baseAddress = null;
            }
            else
            {
               this.baseAddress = new Uri( value );
            }
         }
      }

      public RequestCachePolicy CachePolicy
      {
         get
         {
            throw GetMustImplement();
         }
         set
         {
            throw GetMustImplement();
         }
      }

      public ICredentials Credentials
      {
         get
         {
            return this.credentials;
         }
         set
         {
            this.credentials = value;
         }
      }

      public System.Text.Encoding Encoding
      {
         get
         {
            return this.encoding;
         }
         set
         {
            if( value == null )
            {
               throw new ArgumentNullException( "Encoding" );
            }
            this.encoding = value;
         }
      }

      public WebHeaderCollection Headers
      {
         get
         {
            if( this.headers == null )
            {
               this.headers = new WebHeaderCollection();
            }
            return this.headers;
         }
         set
         {
            this.headers = value;
         }
      }

      public bool IsBusy
      {
         get
         {
            return this.is_busy;
         }
      }

      public IWebProxy Proxy
      {
         get
         {
            return this.proxy;
         }
         set
         {
            this.proxy = value;
         }
      }

      public NameValueCollection QueryString
      {
         get
         {
            if( this.queryString == null )
            {
               this.queryString = new NameValueCollection();
            }
            return this.queryString;
         }
         set
         {
            this.queryString = value;
         }
      }

      public WebHeaderCollection ResponseHeaders
      {
         get
         {
            return this.responseHeaders;
         }
      }

      public bool UseDefaultCredentials
      {
         get
         {
            throw GetMustImplement();
         }
         set
         {
            throw GetMustImplement();
         }
      }
   }

}
