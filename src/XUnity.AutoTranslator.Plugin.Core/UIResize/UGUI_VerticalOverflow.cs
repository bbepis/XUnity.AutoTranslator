using System;
using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UGUI_VerticalOverflow : IUGUI_VerticalOverflow
   {
      //private static readonly Dictionary<string, int?> _values = new Dictionary<string, int?>( StringComparer.OrdinalIgnoreCase )
      //{
      //   { "truncate", 0 },
      //   { "overflow", 1 },
      //};

      private int? _mode;

      public UGUI_VerticalOverflow( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "UGUI_VerticalOverflow requires one argument." );

         //_values.TryGetValue( args[ 0 ], out _mode );
         _mode = (int)EnumHelper.GetValues( UnityTypes.VerticalWrapMode, args[ 0 ] );
      }

      public int? GetMode()
      {
         return _mode;
      }
   }
}
