namespace EverythingNet.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using EverythingNet.Interfaces;
    using SharpBoxesCore.Helpers;

    public static class EverythingState
    {
        public enum StartMode
        {
            Install,
            Service,
        }

        public static bool IsStarted()
        {
            Version version = GetVersion();

            return version.Major > 0;
        }

        public static bool StartService(bool admin, StartMode mode)
        {
            if (!IsStarted())
            {
                string option = admin ? "-admin" : string.Empty;

                switch (mode)
                {
                    case StartMode.Install:
                        option += " -install-service";
                        break;
                    case StartMode.Service:
                        option += " -startup";
                        break;
                }

                StartProcess(option);

                return IsStarted();
            }

            return true;
        }

        public static bool IsReady()
        {
            return EverythingWrapper.Everything_IsDBLoaded();
        }

        public static Version GetVersion()
        {
            UInt32 major = EverythingWrapper.Everything_GetMajorVersion();
            UInt32 minor = EverythingWrapper.Everything_GetMinorVersion();
            UInt32 build = EverythingWrapper.Everything_GetBuildNumber();
            UInt32 revision = EverythingWrapper.Everything_GetRevision();

            return new Version(
                Convert.ToInt32(major),
                Convert.ToInt32(minor),
                Convert.ToInt32(build),
                Convert.ToInt32(revision)
            );
        }

        public static ErrorCode GetLastError()
        {
            return (ErrorCode)EverythingWrapper.Everything_GetLastError();
        }

        private static void StartProcess(string options)
        {
            string path = Path.GetDirectoryName(
                AppDomain.CurrentDomain.BaseDirectory.PathCombine(@"runtimes\any\native")
            );
            string exePath = Path.GetFullPath(Path.Combine(path, @"Everything.exe"));

            if (System.IO.File.Exists(exePath))
            {
                Process.Start(exePath, options);
            }
            else if (
                System.IO.File.Exists(
                    AppDomain.CurrentDomain.BaseDirectory.PathCombine(@"Everything.exe")
                )
            )
            {
                Process.Start(
                    AppDomain.CurrentDomain.BaseDirectory.PathCombine(@"Everything.exe"),
                    options
                );
            }
        }
    }
}
