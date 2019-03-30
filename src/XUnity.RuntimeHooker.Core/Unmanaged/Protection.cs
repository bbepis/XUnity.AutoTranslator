using System;
using System.Collections.Generic;
using System.Text;

namespace XUnity.RuntimeHooker.Unmanaged
{
   /// <summary>A bit-field of flags for protections</summary>
   [Flags]
   internal enum Protection
   {
      /// <summary>No access</summary>
      PAGE_NOACCESS = 0x01,
      /// <summary>Read only</summary>
      PAGE_READONLY = 0x02,
      /// <summary>Read write</summary>
      PAGE_READWRITE = 0x04,
      /// <summary>Write copy</summary>
      PAGE_WRITECOPY = 0x08,
      /// <summary>No access</summary>
      PAGE_EXECUTE = 0x10,
      /// <summary>Execute read</summary>
      PAGE_EXECUTE_READ = 0x20,
      /// <summary>Execute read write</summary>
      PAGE_EXECUTE_READWRITE = 0x40,
      /// <summary>Execute write copy</summary>
      PAGE_EXECUTE_WRITECOPY = 0x80,
      /// <summary>guard</summary>
      PAGE_GUARD = 0x100,
      /// <summary>No cache</summary>
      PAGE_NOCACHE = 0x200,
      /// <summary>Write combine</summary>
      PAGE_WRITECOMBINE = 0x400
   }
}
