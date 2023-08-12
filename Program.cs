using ObjectStoreE;
using System.Reflection;

namespace Supdate
{
    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                string baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (args.Length == 0) // Startup mode
                {
                    if (File.Exists(Path.Combine(baseLocation, "SupdateInstallFinalise.ose")))
                    {
                        CleanupManager.FinaliseInstall(Path.Combine(baseLocation, "SupdateInstallFinalise.ose"));
                        File.Delete(Path.Combine(baseLocation, "SupdateInstallFinalise.ose"));
                    }
                    if (File.Exists(Path.Combine(baseLocation, "SupdateInstallFinaliseTempData.ose")))
                    {
                        CleanupManager.ClearTempData(Path.Combine(baseLocation, "SupdateInstallFinaliseTempData.ose"));
                        File.Delete(Path.Combine(baseLocation, "SupdateInstallFinaliseTempData.ose"));

                    }

                    if (File.Exists(Path.Combine(baseLocation, "SupdateStartup.ose")))
                    {
                        string startup = (string)Automatic.ConvertRegionToObject(Region.CreateSingleRegionByPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SupdateStartup.ose")));
                        IPackage startupPackage = PackageLoader.LoadIPackageFromPath(startup) ?? throw new Exception("Couldn't load startup Ipackage");
                        startupPackage.StartInstance(baseLocation);
                        return;
                    }

                }

                ArgCheck.InterpretArguments(ArgCheck.TokeniseArgs(args, ArgCheck.argCommandsDefinitions));
            }
            catch (ExitException)
            {
                return;
            }
            return;
        }
    }
}
