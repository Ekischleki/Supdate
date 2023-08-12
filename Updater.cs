using ObjectStoreE;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Supdate
{
    public class Updater
    {
        /// <summary>
        /// Checks for updates and automatically installes an update if there is one.
        /// </summary>
        /// <param name="package"></param>
        /// <returns>Whether the client was updated</returns>
        public enum UpdateEndCode
        {
            ClientOnLatestVersion,
            UpdateSuccess,
            FailedAtVerCheck,
            ClientOrServerOffline,
            FailedAtPackageDownload,
            PackageInvalid,
            InstallFailed
        }
        public static UpdateEndCode CheckUpdate(IPackage oldIPackage, string oldIPackageLocation)

        {


            if (oldIPackage.CurrentVersion.AlwaysUpdate)
            {
                return Update(oldIPackage, oldIPackageLocation);
            }

            if (!PingHost(oldIPackage.LatestVersionDownload))
            {
                ConsoleLog.Error("IPackage server or client is offline.");
                return UpdateEndCode.ClientOrServerOffline;
            }
            if (!PingHost(oldIPackage.LatestPackageDownload))
            {
                ConsoleLog.Error("IPackage server or client is offline.");
                return UpdateEndCode.ClientOrServerOffline;
            }

            HttpClient httpClient = new();



            var latestVersionString = httpClient.GetStringAsync(oldIPackage.LatestVersionDownload).Result;
            if (latestVersionString == null)
            {
                ConsoleLog.Warn("The latest version returned null - assuming there's an update");
                return Update(oldIPackage, oldIPackageLocation);
            }
            Version latestVersion = new(latestVersionString);
            if ((latestVersion > oldIPackage.CurrentVersion) ?? true)
            {
                return Update(oldIPackage, oldIPackageLocation);
            }
            if ((latestVersion < oldIPackage.CurrentVersion) ?? throw new Exception("Always update check has alreay been done"))
            {
                ConsoleLog.Warn("Current version is newer than latest oldIPackage. If this is a dev version, that's ok.");
            }
            return UpdateEndCode.ClientOnLatestVersion;


        }

        public static UpdateEndCode Update(IPackage oldIPackage, string oldPackageLocation, HttpClient httpClient = null)
        {
            if (httpClient == null)
                httpClient = new();

            string extractDir = Path.GetTempPath();



            string? newIPackagePath = Path.GetTempPath();
            try
            {
                ConsoleLog.Log("Downloading oldIPackage");
                byte[] latestPackageBinary = httpClient.GetByteArrayAsync(oldIPackage.LatestPackageDownload).Result;
                //There are no methods for extracting from a byte array directly
                ConsoleLog.Log("Extracting oldIPackage");
                File.WriteAllBytes(Path.Combine(extractDir, "Package.zip"), latestPackageBinary);
                Directory.CreateDirectory(Path.Combine(extractDir, "Package"));
                ZipFile.ExtractToDirectory(Path.Combine(extractDir, "Package.zip"), Path.Combine(extractDir, "Package"));
                ConsoleLog.Log("Loading latest ipackage");
                IPackage? latestIPackage;
                if (!File.Exists(Path.Combine(extractDir, "Package", "Supdate.dll")))
                {
                    ConsoleLog.Error("There's no Supdate.dll in the pagage, continuing with old ipackage");
                    latestIPackage = oldIPackage;
                    newIPackagePath = null;
                }
                else
                {
                    File.Copy(Path.Combine(extractDir, "Package", "Supdate.dll"), Path.Combine(newIPackagePath, "Supdate.dll"));
                    latestIPackage = PackageLoader.LoadIPackageFromPath(Path.Combine(newIPackagePath, "Supdate.dll"));
                    if (latestIPackage == null)
                    {
                        ConsoleLog.Error("Supdate.dll failed to load, continuing with old ipackage");
                        latestIPackage = oldIPackage;
                    }
                }
                ConsoleLog.Log("Moving base level files");
                string basePath = Path.Combine(extractDir, "Base");

                Directory.CreateDirectory(basePath);

                foreach (string file in latestIPackage.BaseLevelData)
                {
                    string actualFilePath = Path.Combine(extractDir, "Package", file);
                    string filename = Path.GetFileName(actualFilePath);
                    if (actualFilePath.Equals(Path.Combine(extractDir, "Package", "Supdate.dll"), StringComparison.CurrentCultureIgnoreCase))
                        throw new EndInstallException("The Supdate.dll can't be in the base directory.", UpdateEndCode.InstallFailed);

                    File.Move(actualFilePath, Path.Combine(basePath, filename));
                }

                ConsoleLog.Log("Moving Package to destination");

                string installPath = Path.Combine(latestIPackage.InstallPath, latestIPackage.CurrentVersion.ToString());

                Directory.CreateDirectory(installPath);
                Directory.Move(Path.Combine(extractDir, "oldIPackage"), installPath);



                ConsoleLog.Log("Running latest Package install script");
                latestIPackage.InstallScript(installPath);

                if (!oldIPackage.isFirstInstall)
                {
                    ConsoleLog.Log("Saving data for cleanup");
                    File.WriteAllText(Path.Combine(installPath, "SupdateCleanupData.ose"), Automatic.ConvertObjectToRegion((oldPackageLocation, Path.Combine(installPath, "Supdate.dll")), "DataSave").RegionSaveString);
                }
                Console.WriteLine("Copying base dir files");
                string installerName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
                bool containsNewInstaller = false; 
                foreach (string file in Directory.EnumerateFiles(basePath))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.Equals(installerName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        containsNewInstaller = true;
                        continue;
                    }
                    if (File.Exists(Path.Combine(latestIPackage.InstallPath, fileName)))
                        File.Delete(Path.Combine(latestIPackage.InstallPath, fileName));
                    

                    
                }


                    ConsoleLog.Log("Cleaning up...");
                Directory.Delete(extractDir, true);
                if (newIPackagePath != null)
                    Directory.Delete(newIPackagePath, true);
                return UpdateEndCode.UpdateSuccess;
            }
            catch (Exception ex)
            {
                ConsoleLog.Log("Cleaning up...");
                Directory.Delete(extractDir, true);
                if (newIPackagePath != null)
                    Directory.Delete(newIPackagePath, true);

                ConsoleLog.Fatality(ex.Message);
                if (ex is EndInstallException endInstallException)
                {
                    return endInstallException.UpdateEndCode;
                }
                return UpdateEndCode.InstallFailed;
            }


        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping? pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

    }
}
