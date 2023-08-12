using System.Reflection;

namespace Supdate
{
    internal class PackageLoader
    {

        public static IPackage? LoadIPackageFromPath(string path)
        {
            try
            {
                return GetPluginsFromAssembly(LoadSupdatePackageAssembly(path));
            }
            catch (Exception ex)
            {
                ConsoleLog.Error($"Couldn't load package because: {ex.Message}");
                return null;
            }
        }
        public static Assembly LoadSupdatePackageAssembly(string relativePath) // https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
        {
            return Assembly.LoadFile(relativePath);
        }
        public static IPackage? GetPluginsFromAssembly(Assembly assembly)
        {
            try
            {
                IPackage? foundPackage = null;
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IPackage).IsAssignableFrom(type))
                    {
                        if (foundPackage != null)
                            throw new Exception("There can only be one package defined.");
                        foundPackage = Activator.CreateInstance(type) as IPackage;

                    }
                }
                return foundPackage ?? throw new Exception("There were no derrived IPackage classes defined.");
            } catch (Exception ex)
            {
                ConsoleLog.Error($"Couldn't load package because: {ex.Message}");
                return null;
            }

        }
    }
}
