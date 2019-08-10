using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Http.ExtProtocol
{
   /// <summary>
   /// Web client that can be used to send web requests in a unity compatible way.
   /// </summary>
   public class XUnityWebClient : WebClient
   {
      private HttpStatusCode? _responseCode;
      private CookieCollection _responseCookies;

      private CookieContainer _requestCookies;
      private WebHeaderCollection _requestHeaders;

      /// <summary>
      /// Default constructor.
      /// </summary>
      public XUnityWebClient()
      {
         Encoding = Encoding.UTF8;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="address"></param>
      /// <returns></returns>
      protected override WebRequest GetWebRequest( Uri address )
      {
         var request = base.GetWebRequest( address );
         SetRequestVariables( request );
         return request;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="request"></param>
      /// <param name="result"></param>
      /// <returns></returns>
      protected override WebResponse GetWebResponse( WebRequest request, IAsyncResult result )
      {
         WebResponse response = base.GetWebResponse( request, result );
         SetResponseVariables( response );
         return response;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      protected override WebResponse GetWebResponse( WebRequest request )
      {
         WebResponse response = base.GetWebResponse( request );
         SetResponseVariables( response );
         return response;
      }

      private void SetRequestVariables( WebRequest r )
      {
         var httpRequest = r as HttpWebRequest;
         if( httpRequest != null )
         {
            if( _requestCookies != null )
            {
               httpRequest.CookieContainer = _requestCookies;
            }
            if( _requestHeaders != null )
            {
               Headers = _requestHeaders;
            }
            httpRequest.ReadWriteTimeout = (int)( 60 * 1000 ) - 10000;
            httpRequest.Timeout = (int)( 60 * 1000 ) - 5000;
         }
      }

      private void SetResponseVariables( WebResponse r )
      {
         var httpResponse = r as HttpWebResponse;
         if( httpResponse != null )
         {
            _responseCode = httpResponse.StatusCode;
            _responseCookies = httpResponse.Cookies;
         }
      }

      /// <summary>
      /// Sends a web request and received a response object that can be yielded.
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      public async Task<XUnityWebResponse> SendAsync( XUnityWebRequest request )
      {
         var response = new XUnityWebResponse();

         _requestCookies = request.Cookies;
         _requestHeaders = request.Headers;

         if( request.Data == null )
         {
            Exception error = null;
            string result = null;
            try
            {
               result = await this.DownloadStringTaskAsync( request.Address );
            }
            catch( Exception e )
            {
               error = e;
            }

            response.SetCompleted( _responseCode.Value, result, ResponseHeaders, _responseCookies, error );
         }
         else
         {
            Exception error = null;
            string result = null;
            try
            {
               result = await this.UploadStringTaskAsync( request.Address, request.Method, request.Data );
            }
            catch( Exception e )
            {
               error = e;
            }

            response.SetCompleted( _responseCode.Value, result, ResponseHeaders, _responseCookies, error );
         }

         return response;
      }
   }
}
