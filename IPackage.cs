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
        /// Enables the updater to update while the package is still running. This means that the preferred update path must be replaced.
        /// The package will be running in a folder that represents its version and the latest package will be installed in a folder that represents its version. 
        /// </summary>
        
        string StartExePath { get; }
        /// <summary>
        /// Filenames that are supposed to be in the base directory and not in the package directory. If these files already exist, they will be overwrittern
        /// </summary>
        string[] BaseLevelData { get; }

        /// <summary>
        /// This will run after all package files have been extracted to the install path.
        /// This script is located on the newest package in Supdate.dll. This package is not supposed to start the main exe but just to finalize the install.
        /// </summary>
        /// <param name="extractedTo">The package path</param>
        
        void InstallScript(string extractedTo);
        /// <summary>
        /// This script will be called when this Ipackage has become obsoleete and has been replaced by a newer version
        /// This is supposed to remove temporary data the newer version. The package data will be disposed off automatically
        /// DO NOT DELETE SAVE DATA THE NEWER VERSION MIGHT NEED. You can delete save data the newer version doesn't need anymore.
        /// The new base directory files will not have been copyed by now.
        /// </summary>
        /// <param name="extractedTo">The package path</param>
        void Cleanup(string extractedTo);

        /// <summary>
        /// Use this for packages that are only meant as an installer and not an updater.
        /// If this option is choosen there will be no cleanup
        /// </summary>
        bool isFirstInstall { get; }
    }
}
