using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    /// Determines if WIF is installed on the machine.
    /// </summary>
    public static class WifDetector
    {
        /// <summary>
        /// Gets a value indicating that WIF appears to be installed.
        /// </summary>
        public static bool WifInstalled { get; private set; }
        static WifDetector()
        {
            WifInstalled = IsWifInstalled();
        }

        public static void CheckForWifInstall()
        {
            return;
            if (!WifInstalled)
            {
                //throw new Exception("This application requires the Windows Identity Foundation 3.5 runtime to be installed on your machine.");
                var msg = "This application requires the Windows Identity Foundation 3.5 runtime to be installed on your machine.";
                VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, msg, "WIF 3.5 Required", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                throw new Exception(msg);
            }
        }

        private static bool IsWifInstalled()
        {
            return true;  // TODO temp until I get this fixed.
            try
            {

                //return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                //                                "Reference Assemblies\\Microsoft\\Windows Identity Foundation\\v3.5\\Microsoft.IdentityModel.dll"));
                //The registry approach seems simpler.
                using (var registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows Identity Foundation") ??
                                         Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows Identity Foundation"))
                {
                    return registryKey != null;
                }

                //testAssembly = System.Reflection.Assembly.LoadFrom(dllname);

                //if (!testAssembly.GlobalAssemblyCache)
                //{
                //    // not in gac
                //}


            }
            catch
            {
                //if we don't have permissions or something, this probably isn't a developer machine, hopefully the server admins will figure out the pre-reqs.
                return true;
            }
        }
    }

    //public static class GacUtil
    //{
    //    public static bool IsAssemblyInGAC(string assemblyFullName)
    //    {
    //        try
    //        {
    //            return Assembly.ReflectionOnlyLoad(assemblyFullName)
    //                           .GlobalAssemblyCache;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    public static bool IsAssemblyInGAC(Assembly assembly)
    //    {
    //        return assembly.GlobalAssemblyCache;
    //    }
    //}
}
