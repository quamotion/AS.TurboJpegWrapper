using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    internal static class LibraryResolver
    {
        static LibraryResolver()
        {
#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NET45
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
#endif
        }

        public static void EnsureRegistered()
        {
            // Dummy call to trigger the static constructor
        }

#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NET45
        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != TurboJpegImport.UnmanagedLibrary)
            {
                return IntPtr.Zero;
            }

            // We ship "turbojpeg.dll" and "libturbojpeg.dylib" as part of the NuGet package,
            // so there's nothing left to do for Windows and macOS.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return IntPtr.Zero;
            }

            // On Debian & Ubuntu, there are two out-of-the-box packages:
            // - libjpeg-turbo8 is the libjpeg-turbo, masquerading as the
            //   default jpeg library. It installs as libjpeg.so.8 and is
            //   usually a slightly outdated version.
            // - libturbojpeg is the same library, but usually a more
            //   recent version. It installs as libturbojpeg.so.0
            //
            // On CentOS, the same applies, but the names are:
            // libjpeg.so.62
            // libturbojpeg.so.0
            //
            // Try the specialized version first, then fall back to the general
            // purpose library.
            IntPtr lib = IntPtr.Zero;

            if (NativeLibrary.TryLoad("libturbojpeg.so.0", out lib))
            {
                return lib;
            }

            // Debian/Ubuntu general-purpose jpeg library (but usually
            // libjpeg-turbo)
            if (NativeLibrary.TryLoad("libjpeg.so.8", out lib))
            {
                return lib;
            }

            // CentOS/RHEL general-purpose jpeg library (but usually
            // libjpeg-turbo)
            if (NativeLibrary.TryLoad("libjpeg.so.62", out lib))
            {
                return lib;
            }

            return IntPtr.Zero;
        }
#endif
    }
}