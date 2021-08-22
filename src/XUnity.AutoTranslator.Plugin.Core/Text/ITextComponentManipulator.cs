using System.Text;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal interface ITextComponentManipulator
   {
      string GetText( object ui );

      void SetText( object ui, string text );
   }
}
