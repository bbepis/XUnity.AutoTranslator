using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using XUnity.RuntimeHooker.Unmanaged;

namespace XUnity.RuntimeHooker.Core.Utilities
{
   public static class MemoryHelper
   {
      private static readonly HashSet<PlatformID> WindowsPlatformIDSet = new HashSet<PlatformID> { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE };
      private static readonly bool IsWindows = WindowsPlatformIDSet.Contains( Environment.OSVersion.Platform );

      private static readonly byte[] Prefix = new byte[] { 0xe9 };
      private static readonly byte[] Instruction_1_64bit = new byte[] { 0x48, 0xB8 };
      private static readonly byte[] Instruction_2_64bit = new byte[] { 0xFF, 0xE0 };
      private const byte Instruction_1_32bit = 0x68;
      private const byte Instruction_2_32bit = 0xc3;
      private const int MemoryRequiredForJumpInstruction_32bit = 6;
      private const int MemoryRequiredForJumpInstruction_64bit = 12;

      public static long GetMethodStartLocation( MethodBase method )
      {
         var handle = GetRuntimeMethodHandle( method );
         try
         {
            RuntimeHelpers.PrepareMethod( handle );
         }
         catch( Exception )
         {
         }

         return handle.GetFunctionPointer().ToInt64();
      }


      public static void WriteJump( bool unprotect, long memory, long destination )
      {
         if( unprotect )
         {
            UnprotectMemoryPage( memory );
         }

         if( IntPtr.Size == sizeof( long ) )
         {
            if( CompareBytes( memory, Prefix ) )
            {
               var offset = ReadInt( memory + 1 );
               memory += 5 + offset;
            }

            memory = WriteBytes( memory, Instruction_1_64bit );
            memory = WriteLong( memory, destination );
            memory = WriteBytes( memory, Instruction_2_64bit );
         }
         else
         {
            memory = WriteByte( memory, Instruction_1_32bit );
            memory = WriteInt( memory, (int)destination );
            memory = WriteByte( memory, Instruction_2_32bit );
         }
      }

      public static byte[] GetInstructionsAtLocationRequiredToWriteJump( long location )
      {
         List<byte> array = new List<byte>();

         var flag = IntPtr.Size == sizeof( long );
         if( flag )
         {
            var flag2 = CompareBytes( location, Prefix );

            if( flag2 )
            {
               var num = ReadInt( location + 1L );
               location += 5 + num;
            }
            for( var i = 0 ; i < MemoryRequiredForJumpInstruction_64bit ; i++ )
            {
               array.Add( ReadByte( location + i ) );
            }
         }
         else
         {
            for( var j = 0 ; j < MemoryRequiredForJumpInstruction_32bit ; j++ )
            {
               array.Add( ReadByte( location + j ) );
            }
         }

         return array.ToArray();
      }

      public static void RestoreInstructionsAtLocation( bool unprotect, long location, byte[] array )
      {
         if( unprotect )
         {
            UnprotectMemoryPage( location );
         }
         
         var flag = IntPtr.Size == sizeof( long );
         if( flag )
         {
            if( CompareBytes( location, Prefix ) )
            {
               var num = ReadInt( location + 1L );
               location += 5 + num;
            }
         }
         
         foreach( var b in array )
         {
            location = WriteByte( location, b );
         }
      }

      public static unsafe bool CompareBytes( long memory, byte[] values )
      {
         var p = (byte*)memory;
         foreach( var value in values )
         {
            if( value != *p ) return false;
            p++;
         }
         return true;
      }
      public static unsafe byte ReadByte( long memory )
      {
         var p = (byte*)memory;
         return *p;
      }

      public static unsafe int ReadInt( long memory )
      {
         var p = (int*)memory;
         return *p;
      }

      public static unsafe long ReadLong( long memory )
      {
         var p = (long*)memory;
         return *p;
      }

      public static unsafe long WriteByte( long memory, byte value )
      {
         var p = (byte*)memory;
         *p = value;
         return memory + sizeof( byte );
      }

      public static unsafe long WriteBytes( long memory, byte[] values )
      {
         foreach( var value in values )
            memory = WriteByte( memory, value );
         return memory;
      }

      public static unsafe long WriteInt( long memory, int value )
      {
         var p = (int*)memory;
         *p = value;
         return memory + sizeof( int );
      }

      public static unsafe long WriteLong( long memory, long value )
      {
         var p = (long*)memory;
         *p = value;
         return memory + sizeof( long );
      }

      private static void UnprotectMemoryPage( long memory )
      {
         if( IsWindows )
         {
            var success = Kernel32.VirtualProtect( new IntPtr( memory ), new UIntPtr( 1 ), Protection.PAGE_EXECUTE_READWRITE, out var _ignored );
            if( success == false )
               throw new System.ComponentModel.Win32Exception();
         }
      }

      private static RuntimeMethodHandle GetRuntimeMethodHandle( MethodBase method )
      {
         if( method is DynamicMethod )
         {
            var noninternalInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            // DynamicMethod actually generates its m_methodHandle on-the-fly and therefore
            // we should call GetMethodDescriptor to force it to be created
            //
            var m_GetMethodDescriptor = typeof( DynamicMethod ).GetMethod( "GetMethodDescriptor", noninternalInstance );
            if( m_GetMethodDescriptor != null )
               return (RuntimeMethodHandle)m_GetMethodDescriptor.Invoke( method, new object[ 0 ] );

            // .Net Core
            var f_m_method = typeof( DynamicMethod ).GetField( "m_method", noninternalInstance );
            if( f_m_method != null )
               return (RuntimeMethodHandle)f_m_method.GetValue( method );

            // Mono
            var f_mhandle = typeof( DynamicMethod ).GetField( "mhandle", noninternalInstance );
            return (RuntimeMethodHandle)f_mhandle.GetValue( method );
         }

         return method.MethodHandle;
      }
   }
}
