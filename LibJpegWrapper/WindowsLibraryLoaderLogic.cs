using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TurboJpegWrapper
{
        internal class WindowsLibraryLoaderLogic
        {
            private static readonly object Lock = new object();
            private static readonly IDictionary<string, IntPtr> SearchDirectoryPaths = new Dictionary<string, IntPtr>();

            static WindowsLibraryLoaderLogic()
            {
                SetDefaultDllDirectories((uint)DirectoryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
            }

       public string FixUpLibraryName(string fileName)
            {
                if (!String.IsNullOrEmpty(fileName) && !fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    return fileName + ".dll";
                return fileName;
            }

            public void SetDllPath(string lpPathName)
            {
                SetDllDirectory(lpPathName);
            }

            public void AddDllPath(string lpPathName)
            {
                if (string.IsNullOrEmpty(lpPathName))
                    throw new ArgumentNullException("lpPathName");
                if (!Path.IsPathRooted(lpPathName))
                    throw new ArgumentException("Path must be absolute", "lpPathName");

                var key = lpPathName.ToLowerInvariant();
                lock (Lock)
                {
                    if (SearchDirectoryPaths.ContainsKey(key))
                        return;

                    var cookie = AddDllDirectory(key);

                    if (cookie == IntPtr.Zero)
                    {
                        var error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error);
                    }

                    SearchDirectoryPaths.Add(key, cookie);
                }
            }

            public void RemoveDllPath(string lpPathName)
            {
                var key = lpPathName.ToLowerInvariant();
                lock (Lock)
                {
                    if (!SearchDirectoryPaths.ContainsKey(key))
                        return;
                    var cookie = SearchDirectoryPaths[key];
                    SearchDirectoryPaths.Remove(key);
                    RemoveDllDirectory(cookie);
                }
            }

            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool SetDefaultDllDirectories(uint directoryFlags);

            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool RemoveDllDirectory(IntPtr directoryCookie);

            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr AddDllDirectory(string lpPathName);

            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool SetDllDirectory(string lpPathName);

            [DllImport("kernel32", EntryPoint = "LoadLibrary", CallingConvention = CallingConvention.Winapi,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            private static extern IntPtr WindowsLoadLibrary(string dllPath);

            [DllImport("kernel32", EntryPoint = "FreeLibrary", CallingConvention = CallingConvention.Winapi,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            private static extern bool WindowsFreeLibrary(IntPtr handle);

            [DllImport("kernel32", EntryPoint = "GetProcAddress", CallingConvention = CallingConvention.Winapi,
                SetLastError = true)]
            private static extern IntPtr WindowsGetProcAddress(IntPtr handle, string procedureName);

            private static int WindowsGetLastError()
            {
                return Marshal.GetLastWin32Error();
            }


            enum DirectoryFlags : uint
            {
                /// <summary>
                /// If this value is used, the application's installation directory is searched.
                /// </summary>
                LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
                /// <summary>
                /// If this value is used, %windows%\system32 is searched 
                /// </summary>
                LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
                /// <summary>
                /// If this value is used, any path explicitly added using the AddDllDirectory or SetDllDirectory function is searched. If more than one directory has been added, the order in which those directories are searched is unspecified.
                /// </summary>
                LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
                /// <summary>
                /// This value is a combination of <see cref="LOAD_LIBRARY_SEARCH_APPLICATION_DIR"/>, <see cref="LOAD_LIBRARY_SEARCH_SYSTEM32"/>, and <see cref="LOAD_LIBRARY_SEARCH_USER_DIRS"/>.
                /// This value represents the recommended maximum number of directories an application should include in its DLL search path.
                /// </summary>
                LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
            }
        }
}
