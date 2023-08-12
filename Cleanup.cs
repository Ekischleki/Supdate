using ObjectStoreE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Supdate
{
    internal class CleanupManager
    {
        public static void ClearTempData(string cleanupSave)
        {
            ConsoleLog.Log("Loading cleanup object");
            var cleanupObject = (List<string>)Automatic.ConvertRegionToObject(Region.CreateSingleRegionByPath(cleanupSave));
            ConsoleLog.Log("Deleting files");

            foreach (string path in cleanupObject)
            {
                if (File.Exists(path))
                { File.Delete(path); }
                if (Directory.Exists(path))
                { Directory.Delete(path, true); }
            }
        }

        public static void CleanupInstall(string cleanupSave)
        {
            //oldPackageLocation newPackageLocation basePath

            ConsoleLog.Log("Loading cleanup object");
            var cleanupObject = (string)Automatic.ConvertRegionToObject(Region.CreateSingleRegionByPath(cleanupSave));
            ConsoleLog.Log("Loading old package");
            IPackage oldIPackage = PackageLoader.LoadIPackageFromPath(cleanupObject) ?? throw new Exception("Can't complete cleanup");
            ConsoleLog.Log("Checking if old script is still running");
            if (IsActiveFileLock(oldIPackage.InstanceLockPath(Path.GetDirectoryName(cleanupObject))))
                throw new Exception("Can't complete cleanup due to old instance still running.");
            ConsoleLog.Log("Beginning cleanup");
            ConsoleLog.Log("Running cleanup script on old IPackage");
            oldIPackage.Cleanup(Path.GetDirectoryName(cleanupObject));

            ConsoleLog.Log("Deleting old package");
            Directory.Delete(Path.GetDirectoryName(cleanupObject), true);
            
            
            return;

        }

        public static void FinaliseInstall(string save)
        {
            var saveObject = ((string, string))Automatic.ConvertRegionToObject(Region.CreateSingleRegionByPath(save));
            ConsoleLog.Log("Loading new package");

            IPackage newIPackage = PackageLoader.LoadIPackageFromPath(saveObject.Item2) ?? throw new Exception("Can't complete cleanup");

            ConsoleLog.Log("Copying base files");
            bool updatingSupdate = false;
            if (Directory.Exists(saveObject.Item1))
            {

                foreach (string file in Directory.EnumerateFiles(saveObject.Item1))
                {
                    if (file.Equals(Path.GetFileName(Assembly.GetExecutingAssembly().Location), StringComparison.CurrentCultureIgnoreCase))
                    {
                        updatingSupdate = true;
                        continue;
                    }
                    if (File.Exists(Path.Combine(newIPackage.InstallPath, file)))
                        File.Delete(Path.Combine(newIPackage.InstallPath, file));
                    File.Move(Path.Combine(saveObject.Item1, file), Path.Combine(newIPackage.InstallPath, file));

                }

            }
            if (updatingSupdate)
            {
                string tempExe = Updater.CreateTempDir();
                File.Copy(Assembly.GetExecutingAssembly().Location, tempExe, true);
                ProcessStarter.StartProcess(tempExe, $"/r \"{Path.Combine(saveObject.Item1, Path.GetFileName(Assembly.GetExecutingAssembly().Location))}\" \"{Path.Combine(newIPackage.InstallPath, Path.GetFileName(Assembly.GetExecutingAssembly().Location))}\"");
                throw new ExitException();
            }
        }

        public static bool IsActiveFileLock(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            try
            {
                File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
