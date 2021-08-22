#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shell32
{
   [ComImport, Guid( "286E6F1B-7113-4355-9562-96B7E9D64C54" ), CoClass( typeof( ShellClass ) )]
   public interface Shell : IShellDispatch4 { }

   [ComImport, TypeLibType( (short)2 ), ClassInterface( (short)0 ), Guid( "13709620-C279-11CE-A49E-444553540000" )]
   public class ShellClass : IShellDispatch4, Shell
   {
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60040000 )]
      public virtual extern void AddToRecent( [In, MarshalAs( UnmanagedType.Struct )] object varFile, [In, Optional, MarshalAs( UnmanagedType.BStr )] string bstrCategory );
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      public virtual extern Folder BrowseForFolder( [In] int Hwnd, [In, MarshalAs( UnmanagedType.BStr )] string Title, [In] int Options, [In, Optional, MarshalAs( UnmanagedType.Struct )] object RootFolder );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030007 )]
      public virtual extern object CanStartStopService( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )]
      public virtual extern void CascadeWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020016 )]
      public virtual extern void ControlPanelItem( [In, MarshalAs( UnmanagedType.BStr )] string bstrDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      public virtual extern void EjectPC();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      public virtual extern void Explore( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050002 )]
      public virtual extern object ExplorerPolicy( [In, MarshalAs( UnmanagedType.BStr )] string bstrPolicyName );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      public virtual extern void FileRun();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020014 )]
      public virtual extern void FindComputer();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020013 )]
      public virtual extern void FindFiles();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030002 )]
      public virtual extern void FindPrinter( [In, Optional, MarshalAs( UnmanagedType.BStr )] string Name, [In, Optional, MarshalAs( UnmanagedType.BStr )] string location, [In, Optional, MarshalAs( UnmanagedType.BStr )] string model );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050003 )]
      public virtual extern bool GetSetting( [In] int lSetting );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030003 )]
      public virtual extern object GetSystemInformation( [In, MarshalAs( UnmanagedType.BStr )] string Name );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020012 )]
      public virtual extern void Help();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )]
      public virtual extern int IsRestricted( [In, MarshalAs( UnmanagedType.BStr )] string Group, [In, MarshalAs( UnmanagedType.BStr )] string Restriction );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030006 )]
      public virtual extern object IsServiceRunning( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      public virtual extern void MinimizeAll();
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )]
      public virtual extern Folder NameSpace( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      public virtual extern void Open( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020015 )]
      public virtual extern void RefreshMenu();
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030004 )]
      public virtual extern object ServiceStart( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030005 )]
      public virtual extern object ServiceStop( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020010 )]
      public virtual extern void SetTime();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030001 )]
      public virtual extern void ShellExecute( [In, MarshalAs( UnmanagedType.BStr )] string File, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vArgs, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vDir, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOperation, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vShow );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030008 )]
      public virtual extern object ShowBrowserBar( [In, MarshalAs( UnmanagedType.BStr )] string bstrClsid, [In, MarshalAs( UnmanagedType.Struct )] object bShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      public virtual extern void ShutdownWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      public virtual extern void Suspend();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      public virtual extern void TileHorizontally();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )]
      public virtual extern void TileVertically();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050001 )]
      public virtual extern void ToggleDesktop();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020011 )]
      public virtual extern void TrayProperties();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      public virtual extern void UndoMinimizeALL();
      [return: MarshalAs( UnmanagedType.IDispatch )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      public virtual extern object Windows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050000 )]
      public virtual extern void WindowsSecurity();

      [DispId( 0x60020000 )]
      public extern object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      public extern object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [DispId( 0x60020000 )]
      extern object Shell32.IShellDispatch4.Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      extern object Shell32.IShellDispatch4.Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
   }

   [ComImport, TypeLibType( (short)0x1050 ), Guid( "D8F015C0-C278-11CE-A49E-444553540000" )]
   public interface IShellDispatch
   {
      [DispId( 0x60020000 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )]
      Folder NameSpace( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      Folder BrowseForFolder( [In] int Hwnd, [In, MarshalAs( UnmanagedType.BStr )] string Title, [In] int Options, [In, Optional, MarshalAs( UnmanagedType.Struct )] object RootFolder );
      [return: MarshalAs( UnmanagedType.IDispatch )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      object Windows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      void Open( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      void Explore( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      void MinimizeAll();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      void UndoMinimizeALL();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      void FileRun();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )]
      void CascadeWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )]
      void TileVertically();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void TileHorizontally();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      void ShutdownWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void Suspend();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void EjectPC();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020010 )]
      void SetTime();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020011 )]
      void TrayProperties();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020012 )]
      void Help();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020013 )]
      void FindFiles();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020014 )]
      void FindComputer();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020015 )]
      void RefreshMenu();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020016 )]
      void ControlPanelItem( [In, MarshalAs( UnmanagedType.BStr )] string bstrDir );
   }

   [ComImport, TypeLibType( (short)0x1050 ), Guid( "A4C6892C-3BA9-11D2-9DEA-00C04FB16162" )]
   public interface IShellDispatch2 : IShellDispatch
   {
      [DispId( 0x60020000 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )]
      Folder NameSpace( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      Folder BrowseForFolder( [In] int Hwnd, [In, MarshalAs( UnmanagedType.BStr )] string Title, [In] int Options, [In, Optional, MarshalAs( UnmanagedType.Struct )] object RootFolder );
      [return: MarshalAs( UnmanagedType.IDispatch )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      object Windows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      void Open( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      void Explore( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      void MinimizeAll();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      void UndoMinimizeALL();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      void FileRun();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )]
      void CascadeWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )]
      void TileVertically();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void TileHorizontally();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      void ShutdownWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void Suspend();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void EjectPC();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020010 )]
      void SetTime();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020011 )]
      void TrayProperties();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020012 )]
      void Help();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020013 )]
      void FindFiles();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020014 )]
      void FindComputer();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020015 )]
      void RefreshMenu();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020016 )]
      void ControlPanelItem( [In, MarshalAs( UnmanagedType.BStr )] string bstrDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )]
      int IsRestricted( [In, MarshalAs( UnmanagedType.BStr )] string Group, [In, MarshalAs( UnmanagedType.BStr )] string Restriction );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030001 )]
      void ShellExecute( [In, MarshalAs( UnmanagedType.BStr )] string File, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vArgs, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vDir, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOperation, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030002 )]
      void FindPrinter( [In, Optional, MarshalAs( UnmanagedType.BStr )] string Name, [In, Optional, MarshalAs( UnmanagedType.BStr )] string location, [In, Optional, MarshalAs( UnmanagedType.BStr )] string model );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030003 )]
      object GetSystemInformation( [In, MarshalAs( UnmanagedType.BStr )] string Name );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030004 )]
      object ServiceStart( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030005 )]
      object ServiceStop( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030006 )]
      object IsServiceRunning( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030007 )]
      object CanStartStopService( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030008 )]
      object ShowBrowserBar( [In, MarshalAs( UnmanagedType.BStr )] string bstrClsid, [In, MarshalAs( UnmanagedType.Struct )] object bShow );
   }

   [ComImport, TypeLibType( (short)0x1050 ), Guid( "177160CA-BB5A-411C-841D-BD38FACDEAA0" )]
   public interface IShellDispatch3 : IShellDispatch2
   {
      [DispId( 0x60020000 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )]
      Folder NameSpace( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      Folder BrowseForFolder( [In] int Hwnd, [In, MarshalAs( UnmanagedType.BStr )] string Title, [In] int Options, [In, Optional, MarshalAs( UnmanagedType.Struct )] object RootFolder );
      [return: MarshalAs( UnmanagedType.IDispatch )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      object Windows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      void Open( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      void Explore( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      void MinimizeAll();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      void UndoMinimizeALL();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      void FileRun();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )]
      void CascadeWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )]
      void TileVertically();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void TileHorizontally();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      void ShutdownWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void Suspend();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void EjectPC();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020010 )]
      void SetTime();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020011 )]
      void TrayProperties();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020012 )]
      void Help();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020013 )]
      void FindFiles();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020014 )]
      void FindComputer();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020015 )]
      void RefreshMenu();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020016 )]
      void ControlPanelItem( [In, MarshalAs( UnmanagedType.BStr )] string bstrDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )]
      int IsRestricted( [In, MarshalAs( UnmanagedType.BStr )] string Group, [In, MarshalAs( UnmanagedType.BStr )] string Restriction );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030001 )]
      void ShellExecute( [In, MarshalAs( UnmanagedType.BStr )] string File, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vArgs, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vDir, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOperation, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030002 )]
      void FindPrinter( [In, Optional, MarshalAs( UnmanagedType.BStr )] string Name, [In, Optional, MarshalAs( UnmanagedType.BStr )] string location, [In, Optional, MarshalAs( UnmanagedType.BStr )] string model );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030003 )]
      object GetSystemInformation( [In, MarshalAs( UnmanagedType.BStr )] string Name );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030004 )]
      object ServiceStart( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030005 )]
      object ServiceStop( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030006 )]
      object IsServiceRunning( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030007 )]
      object CanStartStopService( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030008 )]
      object ShowBrowserBar( [In, MarshalAs( UnmanagedType.BStr )] string bstrClsid, [In, MarshalAs( UnmanagedType.Struct )] object bShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60040000 )]
      void AddToRecent( [In, MarshalAs( UnmanagedType.Struct )] object varFile, [In, Optional, MarshalAs( UnmanagedType.BStr )] string bstrCategory );
   }

   [ComImport, Guid( "EFD84B2D-4BCF-4298-BE25-EB542A59FBDA" ), TypeLibType( (short)0x1050 )]
   public interface IShellDispatch4 : IShellDispatch3
   {
      [DispId( 0x60020000 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )]
      Folder NameSpace( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      Folder BrowseForFolder( [In] int Hwnd, [In, MarshalAs( UnmanagedType.BStr )] string Title, [In] int Options, [In, Optional, MarshalAs( UnmanagedType.Struct )] object RootFolder );
      [return: MarshalAs( UnmanagedType.IDispatch )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      object Windows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      void Open( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      void Explore( [In, MarshalAs( UnmanagedType.Struct )] object vDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      void MinimizeAll();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      void UndoMinimizeALL();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      void FileRun();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )]
      void CascadeWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )]
      void TileVertically();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void TileHorizontally();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      void ShutdownWindows();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void Suspend();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void EjectPC();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020010 )]
      void SetTime();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020011 )]
      void TrayProperties();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020012 )]
      void Help();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020013 )]
      void FindFiles();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020014 )]
      void FindComputer();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020015 )]
      void RefreshMenu();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020016 )]
      void ControlPanelItem( [In, MarshalAs( UnmanagedType.BStr )] string bstrDir );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )]
      int IsRestricted( [In, MarshalAs( UnmanagedType.BStr )] string Group, [In, MarshalAs( UnmanagedType.BStr )] string Restriction );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030001 )]
      void ShellExecute( [In, MarshalAs( UnmanagedType.BStr )] string File, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vArgs, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vDir, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOperation, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030002 )]
      void FindPrinter( [In, Optional, MarshalAs( UnmanagedType.BStr )] string Name, [In, Optional, MarshalAs( UnmanagedType.BStr )] string location, [In, Optional, MarshalAs( UnmanagedType.BStr )] string model );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030003 )]
      object GetSystemInformation( [In, MarshalAs( UnmanagedType.BStr )] string Name );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030004 )]
      object ServiceStart( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030005 )]
      object ServiceStop( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName, [In, MarshalAs( UnmanagedType.Struct )] object Persistent );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030006 )]
      object IsServiceRunning( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030007 )]
      object CanStartStopService( [In, MarshalAs( UnmanagedType.BStr )] string ServiceName );
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030008 )]
      object ShowBrowserBar( [In, MarshalAs( UnmanagedType.BStr )] string bstrClsid, [In, MarshalAs( UnmanagedType.Struct )] object bShow );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60040000 )]
      void AddToRecent( [In, MarshalAs( UnmanagedType.Struct )] object varFile, [In, Optional, MarshalAs( UnmanagedType.BStr )] string bstrCategory );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050000 )]
      void WindowsSecurity();
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050001 )]
      void ToggleDesktop();
      [return: MarshalAs( UnmanagedType.Struct )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050002 )]
      object ExplorerPolicy( [In, MarshalAs( UnmanagedType.BStr )] string bstrPolicyName );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60050003 )]
      bool GetSetting( [In] int lSetting );
   }

   [ComImport, Guid( "BBCBDE60-C3FF-11CE-8350-444553540000" ), TypeLibType( (short)0x1040 ), DefaultMember( "Title" )]
   public interface Folder
   {
      [DispId( 0 )]
      string Title { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0 )] get; }
      [DispId( 0x60020001 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [DispId( 0x60020002 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; }
      [DispId( 0x60020003 )]
      Folder ParentFolder { [return: MarshalAs( UnmanagedType.Interface )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )]
      FolderItems Items();
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )]
      FolderItem ParseName( [In, MarshalAs( UnmanagedType.BStr )] string bName );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )]
      void NewFolder( [In, MarshalAs( UnmanagedType.BStr )] string bName, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOptions );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )]
      void MoveHere( [In, MarshalAs( UnmanagedType.Struct )] object vItem, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOptions );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )]
      void CopyHere( [In, MarshalAs( UnmanagedType.Struct )] object vItem, [In, Optional, MarshalAs( UnmanagedType.Struct )] object vOptions );
      [return: MarshalAs( UnmanagedType.BStr )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )]
      string GetDetailsOf( [In, MarshalAs( UnmanagedType.Struct )] object vItem, [In] int iColumn );
   }

   [ComImport, Guid( "FAC32C80-CBE4-11CE-8350-444553540000" ), TypeLibType( (short)0x1040 ), DefaultMember( "Name" )]
   public interface FolderItem
   {
      [DispId( 0x60020000 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [DispId( 0 )]
      string Name { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0 )] set; }
      [DispId( 0x60020004 )]
      string Path { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] get; }
      [DispId( 0x60020005 )]
      object GetLink { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020005 )] get; }
      [DispId( 0x60020006 )]
      object GetFolder { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] get; }
      [DispId( 0x60020007 )]
      bool IsLink { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020007 )] get; }
      [DispId( 0x60020008 )]
      bool IsFolder { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] get; }
      [DispId( 0x60020009 )]
      bool IsFileSystem { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020009 )] get; }
      [DispId( 0x6002000a )]
      bool IsBrowsable { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] get; }
      [DispId( 0x6002000b )]
      DateTime ModifyDate { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000b )] set; }
      [DispId( 0x6002000d )]
      int Size { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )] get; }
      [DispId( 0x6002000e )]
      string Type { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )] get; }
   }

   [ComImport, Guid( "317EE249-F12E-11D2-B1E4-00C04F8EEB3E" ), CoClass( typeof( ShellLinkObjectClass ) )]
   public interface ShellLinkObject : IShellLinkDual2
   {

   }

   [ComImport, Guid( "11219420-1768-11D1-95BE-00609797EA4F" ), ClassInterface( (short)0 )]
   public class ShellLinkObjectClass : IShellLinkDual2, ShellLinkObject
   {
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      public virtual extern int GetIconLocation( [MarshalAs( UnmanagedType.BStr )] out string pbs );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      public virtual extern void Resolve( [In] int fFlags );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      public virtual extern void Save( [In, Optional, MarshalAs( UnmanagedType.Struct )] object vWhere );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      public virtual extern void SetIconLocation( [In, MarshalAs( UnmanagedType.BStr )] string bs, [In] int iIcon );
      
      [DispId( 0x60020006 )]
      public extern string Arguments { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] set; }
      [DispId( 0x60020002 )]
      public extern string Description { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] set; }
      [DispId( 0x60020008 )]
      public extern int Hotkey { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] set; }
      [DispId( 0x60020000 )]
      public extern string Path { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] set; }
      [DispId( 0x60020006 )]
      extern string Shell32.IShellLinkDual2.Arguments { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] set; }
      [DispId( 0x60020002 )]
      extern string Shell32.IShellLinkDual2.Description { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] set; }
      [DispId( 0x60020008 )]
      extern int Shell32.IShellLinkDual2.Hotkey { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] set; }
      [DispId( 0x60020000 )]
      extern string Shell32.IShellLinkDual2.Path { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] set; }
      [DispId( 0x6002000a )]
      extern int Shell32.IShellLinkDual2.ShowCommand { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] set; }
      [DispId( 0x60030000 )]
      extern FolderItem Shell32.IShellLinkDual2.Target { [return: MarshalAs( UnmanagedType.Interface )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )] get; }
      [DispId( 0x60020004 )]
      extern string Shell32.IShellLinkDual2.WorkingDirectory { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] set; }
      [DispId( 0x6002000a )]
      public extern int ShowCommand { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] set; }
      [DispId( 0x60030000 )]
      public extern FolderItem Target { [return: MarshalAs( UnmanagedType.Interface )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )] get; }
      [DispId( 0x60020004 )]
      public extern string WorkingDirectory { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] set; }
   }

   [ComImport, Guid( "88A05C00-F000-11CE-8350-444553540000" ), TypeLibType( (short)0x1050 )]
   public interface IShellLinkDual
   {
      [DispId( 0x60020000 )]
      string Path { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] set; }
      [DispId( 0x60020002 )]
      string Description { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] set; }
      [DispId( 0x60020004 )]
      string WorkingDirectory { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] set; }
      [DispId( 0x60020006 )]
      string Arguments { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] set; }
      [DispId( 0x60020008 )]
      int Hotkey { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] set; }
      [DispId( 0x6002000a )]
      int ShowCommand { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] set; }
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void Resolve( [In] int fFlags );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      int GetIconLocation( [MarshalAs( UnmanagedType.BStr )] out string pbs );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void SetIconLocation( [In, MarshalAs( UnmanagedType.BStr )] string bs, [In] int iIcon );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void Save( [In, Optional, MarshalAs( UnmanagedType.Struct )] object vWhere );
   }

   [ComImport, Guid( "317EE249-F12E-11D2-B1E4-00C04F8EEB3E" ), TypeLibType( (short)0x1050 )]
   public interface IShellLinkDual2 : IShellLinkDual
   {
      [DispId( 0x60020000 )]
      string Path { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] set; }
      [DispId( 0x60020002 )]
      string Description { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] set; }
      [DispId( 0x60020004 )]
      string WorkingDirectory { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020004 )] set; }
      [DispId( 0x60020006 )]
      string Arguments { [return: MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] get; [param: In, MarshalAs( UnmanagedType.BStr )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020006 )] set; }
      [DispId( 0x60020008 )]
      int Hotkey { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020008 )] set; }
      [DispId( 0x6002000a )]
      int ShowCommand { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] get; [param: In] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000a )] set; }
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000c )]
      void Resolve( [In] int fFlags );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000d )]
      int GetIconLocation( [MarshalAs( UnmanagedType.BStr )] out string pbs );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000e )]
      void SetIconLocation( [In, MarshalAs( UnmanagedType.BStr )] string bs, [In] int iIcon );
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x6002000f )]
      void Save( [In, Optional, MarshalAs( UnmanagedType.Struct )] object vWhere );
      [DispId( 0x60030000 )]
      FolderItem Target { [return: MarshalAs( UnmanagedType.Interface )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60030000 )] get; }
   }

   [ComImport, Guid( "744129E0-CBE5-11CE-8350-444553540000" ), TypeLibType( (short)0x1040 )]
   public interface FolderItems
   {
      [DispId( 0x60020000 )]
      int Count { [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020000 )] get; }
      [DispId( 0x60020001 )]
      object Application { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020001 )] get; }
      [DispId( 0x60020002 )]
      object Parent { [return: MarshalAs( UnmanagedType.IDispatch )] [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020002 )] get; }
      [return: MarshalAs( UnmanagedType.Interface )]
      [MethodImpl( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), DispId( 0x60020003 )]
      FolderItem Item( [In, Optional, MarshalAs( UnmanagedType.Struct )] object index );
   }
}
