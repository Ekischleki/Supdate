namespace Supdate
{
    public interface IPackage
    {


        /// <summary>
        /// The name of your package
        /// </summary>
        string PackageName { get; }
        /// <summary>
        /// The base path of your package
        /// </summary>
        string InstallPath { get; }
        /// <summary>
        /// The Supdate API version this was written for
        /// </summary>
        int SupportedAPIVersion { get; }
        /// <summary>
        /// The current package version
        /// </summary>
        Version CurrentVersion { get; }
        /// <summary>
        /// The download link to the latest version txt 
        /// </summary>
        string LatestVersionDownload { get; }
        /// <summary>
        /// The download link to the lastest package
        /// </summary>
        string LatestPackageDownload { get; }


        /// <summary>
        /// Filenames that are supposed to be in the base directory and not in the package directory. If these files already exist, they will be overwrittern
        /// </summary>
        string[] BaseLevelData { get; }

        /// <summary>
        /// This will run after all package files have been extracted to the install path.
        /// This method is located on the newest package in SupdateIPackage.dll. This method is not supposed to start the main exe but just to finalise the install.
        /// </summary>
        /// <param name="extractedTo">The package path</param>

        void InstallScript(string versionPath);
        /// <summary>
        /// This method will be called when this Ipackage has become obsoleete and has been replaced by a newer version
        /// This is supposed to remove temporary data the newer version. The package data will be disposed off automatically
        /// DO NOT DELETE SAVE DATA THE NEWER VERSION MIGHT NEED. You can delete save data the newer version doesn't need anymore.
        /// The new base directory files will not have been copyed by now.
        /// </summary>
        /// <param name="extractedTo">The package path</param>
        void Cleanup(string versionPath);

        /// <summary>
        /// Use this for packages that are only meant as an installer and not an updater.
        /// If this option is choosen there will be no cleanup
        /// </summary>
        bool isFirstInstall { get; }
        /// <summary>
        /// This method is needed for checking whether an instance of the program is currently running.
        /// You can create an instance lock by making a streamwriter to the lock path. The streamwriter must stay active for the whole time the program is running. The best way to do this is by creating a static streamwriter in the Main class like this:
        /// private static readonly StreamWriter instanceLock = new("LockPath");
        /// 
        /// </summary>
        /// <param name="versionPath"></param>
        /// <returns>The file location of the instance lock</returns>
        string InstanceLockPath(string versionPath);
        /// <summary>
        /// This is for starting up the actual program. If you need to do something before doing that, you can do it here.
        /// After you initialised the start, you need to start the main exe.
        /// You can do that like this:
        /// <see cref="Supdate.ProcessStarter.StartProcess"/>(startupExe, possibleStartupArgs);
        /// 
        /// </summary>
        /// <param name="versionPath"></param>
        void StartInstance(string versionPath);
    }
}
