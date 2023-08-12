using ObjectStoreE;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;

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
            try
            {



                if (!PingHost(oldIPackage.LatestVersionDownload))
                {
                    ConsoleLog.Error("IPackage server or client is offline.");
                    return UpdateEndCode.ClientOrServerOffline;
                }

                HttpClient httpClient = new();



                var latestVersionString = httpClient.GetStringAsync(oldIPackage.LatestVersionDownload).Result;
                if (latestVersionString == null)
                {
                    ConsoleLog.Warn("The latest version returned null - assuming there's an update");
                    return Update(oldIPackage, oldIPackageLocation, null);
                }
                Version latestVersion = new(latestVersionString);
                if ((latestVersion > oldIPackage.CurrentVersion) ?? true)
                {
                    return Update(oldIPackage, oldIPackageLocation, latestVersion);
                }
                if ((latestVersion < oldIPackage.CurrentVersion) ?? throw new Exception("Always update check has alreay been done"))
                {
                    ConsoleLog.Warn("Current version is newer than latest oldIPackage. If this is a dev version, that's ok.");
                }
                return UpdateEndCode.ClientOnLatestVersion;
            }
            catch (DoFuncException ex)
            {
                ex.action.Invoke();
                return UpdateEndCode.ClientOnLatestVersion;
            }

        }

        public static string CreateTempDir()
        {
            string randomPath;
            do
            {
                randomPath = Path.Combine(Path.GetTempPath(), Random.Shared.NextInt64().ToString());
            } while (Directory.Exists(randomPath));
            Directory.CreateDirectory(randomPath);
            return randomPath;
        }
        public static UpdateEndCode Update(IPackage oldIPackage, string oldPackageLocation, Version latestPackageVersion, HttpClient httpClient = null)
        {
            List<string> tempfileDelete = new();
            if (httpClient == null)
                httpClient = new();

            string extractDir = CreateTempDir();



            string? newIPackagePath = CreateTempDir();
            try
            {
                ConsoleLog.Log("Downloading latest package");
                byte[] latestPackageBinary = httpClient.GetByteArrayAsync(oldIPackage.LatestPackageDownload(latestPackageVersion)).Result;
                //There are no methods for extracting from a byte array directly
                ConsoleLog.Log("Extracting latest package");
                File.WriteAllBytes(Path.Combine(extractDir, "Package.zip"), latestPackageBinary);
                Directory.CreateDirectory(Path.Combine(extractDir, "Package"));
                ZipFile.ExtractToDirectory(Path.Combine(extractDir, "Package.zip"), Path.Combine(extractDir, "Package"));
                ConsoleLog.Log("Loading latest ipackage");
                IPackage? latestIPackage;
                if (!File.Exists(Path.Combine(extractDir, "Package", "SupdateIPackage.dll")))
                {
                    ConsoleLog.Error("There's no SupdateIPackage.dll in the pagage, continuing with old ipackage");
                    latestIPackage = oldIPackage;
                    newIPackagePath = null;
                }
                else
                {
                    File.Copy(Path.Combine(extractDir, "Package", "SupdateIPackage.dll"), Path.Combine(newIPackagePath, "SupdateIPackage.dll"));
                    latestIPackage = PackageLoader.LoadIPackageFromPath(Path.Combine(newIPackagePath, "SupdateIPackage.dll"));
                    if (latestIPackage == null)
                    {
                        ConsoleLog.Error("SupdateIPackage.dll failed to load, continuing with old ipackage");
                        latestIPackage = oldIPackage;
                    }
                }
                ConsoleLog.Log("Moving base level files");
                string basePath = Path.Combine(latestIPackage.InstallPath, "NewBase");
                if (Directory.Exists(basePath))
                {
                    Directory.Delete(basePath, true);
                }
                Directory.CreateDirectory(basePath);

                foreach (string file in latestIPackage.BaseLevelData)
                {
                    string actualFilePath = Path.Combine(extractDir, "Package", file);
                    string filename = Path.GetFileName(actualFilePath);
                    if (actualFilePath.Equals(Path.Combine(extractDir, "Package", "SupdateIPackage.dll"), StringComparison.CurrentCultureIgnoreCase))
                        throw new EndInstallException("The SupdateIPackage.dll can't be in the base directory.", UpdateEndCode.InstallFailed);

                    File.Move(actualFilePath, Path.Combine(basePath, filename));
                }

                ConsoleLog.Log("Moving Package to destination");

                string installPath = Path.Combine(latestIPackage.InstallPath, latestIPackage.CurrentVersion.ToString());

                if (Directory.Exists(installPath))
                {
                    Directory.Delete(installPath, true);
                }

                Directory.Move(Path.Combine(extractDir, "Package"), installPath);



                ConsoleLog.Log("Running latest Package install script");
                latestIPackage.InstallScript(installPath);

                if (!oldIPackage.isFirstInstall)
                {
                    ConsoleLog.Log("Saving data for cleanup and final install");
                    File.WriteAllText(Path.Combine(latestIPackage.InstallPath, "SupdateInstallCleanup.ose"), Automatic.ConvertObjectToRegion(oldPackageLocation, "DataSave").RegionSaveString);

                }
                File.WriteAllText(Path.Combine(latestIPackage.InstallPath, "SupdateStartup.ose"), Automatic.ConvertObjectToRegion((Path.Combine(installPath, "SupdateIPackage.dll"), installPath), "StartupSave").RegionSaveString);
                File.WriteAllText(Path.Combine(latestIPackage.InstallPath, "SupdateInstallFinalise.ose"), Automatic.ConvertObjectToRegion((basePath, Path.Combine(installPath, "SupdateIPackage.dll")), "Base").RegionSaveString);

                ConsoleLog.Log("Cleaning up...");
                tempfileDelete.Add(extractDir);
                if (newIPackagePath != null) //I don't know how to free the recourses so i cant delete the file.
                    tempfileDelete.Add(newIPackagePath);

                File.WriteAllText(Path.Combine(latestIPackage.InstallPath, "SupdateInstallFinaliseTempData.ose"), Automatic.ConvertObjectToRegion(tempfileDelete, "TempData").RegionSaveString);

                return UpdateEndCode.UpdateSuccess;
            }
            catch (Exception ex)
            {
                if (ex is ExitException) throw;
                if (ex is DoFuncException) throw;
                ConsoleLog.Log("Cleaning up...");
                try
                {
                    Directory.Delete(extractDir, true);

                    if (newIPackagePath != null)
                        Directory.Delete(newIPackagePath, true);
                }
                catch
                {
                    ConsoleLog.Error("Full cean up failed, please delete files manually.");
                }
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
