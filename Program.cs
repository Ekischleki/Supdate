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
                string baseLocation = AppContext.BaseDirectory;
                ConsoleLog.Log($"Base location: {baseLocation}");
                ConsoleLog.Log($"Assembly location: {Assembly.GetExecutingAssembly().Location}");
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
                        try
                        {
                            var startup = ((string, string))Automatic.ConvertRegionToObject(Region.CreateSingleRegionByPath(Path.Combine(baseLocation, "SupdateStartup.ose")));
                            IPackage startupPackage = PackageLoader.LoadIPackageFromPath(startup.Item1) ?? throw new Exception("Couldn't load startup Ipackage");
                            startupPackage.StartInstance(startup.Item2);
                            return;
                        } catch (Exception ex)
                        {
                            Console.WriteLine("Failed starting because of fatality.");
                            ConsoleLog.Fatality(ex);
                            Console.ReadKey();
                            return;
                        }
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
