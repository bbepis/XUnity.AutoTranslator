using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Events
{
   public delegate void UnityAction<T0, T1>( T0 arg0, T1 arg1 );

   public delegate void UnityAction<T0>( T0 arg0 );
}
