using System.Diagnostics;

namespace Supdate
{
    internal class ArgCheck
    {
        public static readonly List<ArgDefinition> argCommandsDefinitions = new()
        {
            new("r", "replace arg one with arg two and then start arg 2 with the intend to delete this file", new() {2}, false, true ),
            new("d", "delete arg 1", new() {1}, false, true),
            new("m", "startup normally", new() {0}, false, true),
            new("s", "Update the IPackage specifyed in the arg 1. Usage: /s \"C:\\Path\\To\\IPackage\\SupdateIPackage.dll\"", new() {1} , false, false),
            new("?", "Shows this help", new() {0} , false, false),


        };
        
        public static void InterpretArguments(List<ArgInstance> tokens)
        {
            foreach (ArgInstance token in tokens)
            {
                switch (token.argDefinition.argName)
                {
                    case "m":
                        Program.Main(new string[] {});
                        break;
                    case "?":
                        ShowHelp(argCommandsDefinitions);
                        break;
                    case "s":
                        
                        ConsoleLog.Log( Updater.CheckUpdate(PackageLoader.LoadIPackageFromPath(token.argAttributes[0]), token.argAttributes[0]));
                        break;
                    case "r":
                        for (int i = 0; i < 10; i++)
                            try
                            {
                                ConsoleLog.Log($"Trying to delete \"{token.argAttributes[1]}\" try {i} of 10");
                                File.Delete(token.argAttributes[1]);
                                break;
                            } catch(Exception ex)
                            {
                                ConsoleLog.Error($"Failed to delete arg 1 because {ex.Message}");
                                Thread.Sleep(500);

                            }
                        for (int i = 0; i < 10; i++)
                            try
                            {
                                ConsoleLog.Log($"Trying to move arg {token.argAttributes[0]} to {token.argAttributes[1]} try {i} of 10");
                                File.Move(token.argAttributes[0], token.argAttributes[1]);
                                break;
                            }
                            catch (Exception ex)
                            {
                                ConsoleLog.Error($"Failed to move because {ex.Message}");
                                Thread.Sleep(500);
                            }
                        ProcessStarter.StartProcess(token.argAttributes[0]);
                        break;
                    case "d":
                        for (int i = 0; i < 10; i++)
                            try
                            {
                                ConsoleLog.Log($"Trying to delete arg 1 try {i} of 10");
                                File.Delete(token.argAttributes[0]);
                            }
                            catch (Exception ex)
                            {
                                ConsoleLog.Error($"Failed to delete arg 1 because {ex.Message}");
                                Thread.Sleep(500);

                            }
                        break;

                }
                token.AlreadyUsed = true;
            }
        }

        public static void ShowHelp(List<ArgDefinition> argDefinitions)
        {
            Console.Clear();
            foreach (ArgDefinition argDef in argDefinitions)
            {
                if (argDef.undocumented)
                    continue;
                Console.WriteLine($"\t/{argDef.argName}\t{argDef.argDescription}\n");
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }


        public static List<ArgInstance> TokeniseArgs(string[] args, List<ArgDefinition> argDefinitions)
        {
            ArgInstance? currentArg = null;
            List<ArgInstance> result = new();

            for (int i = 0; i < args.Length; i++)
            {
                string? arg = args[i];
                if (arg[0] == '/' || arg[0] == '-')
                {
                    if (currentArg != null)
                    {
                        result.Add(currentArg);
                    }

                    if (arg.Length < 2)
                    {
                        throw new ArgumentException("There are no empty arguments (\"/\")");
                    }
                    string actualArg = arg[1..];
                    currentArg = new(argDefinitions.FirstOrDefault(x => x.argName == actualArg) ?? throw new ArgumentException($"An argument with the name \"{actualArg}\" doesn't exist. Use the /help argument to see all arguments."));


                }
                else
                {
                    if (currentArg == null)
                        throw new ArgumentException("You must first define an argument before listing attributes.");
                    currentArg.argAttributes.Add(arg);
                }

            }
            if (currentArg != null)
            {
                result.Add(currentArg);
            }
            result.ForEach(a => a.VerifyAttributes());

            return result;
        }

    }
    internal class ArgInstance
    {
        public ArgDefinition argDefinition;
        public List<string> argAttributes;
        private bool alreadyUsed;
        public bool AlreadyUsed
        {
            get
            {
                return alreadyUsed;
            }
            set
            {
                if (alreadyUsed == true)
                    throw new ArgumentException($"The argument {argDefinition.argName} can only be used once");
                alreadyUsed = value;
                if (argDefinition.usableMultibleTimes)
                    alreadyUsed = false;

            }
        }
        public ArgInstance(ArgDefinition argDefinition)
        {
            this.argDefinition = argDefinition;
            argAttributes = new();

        }

        public void VerifyAttributes()
        {
            if (!argDefinition.argAttributes.Contains(argAttributes.Count))
            {
                throw new ArgumentException($"Invalid usage of {argDefinition.argName} argument. Type help to see correct uses");
            }
        }
    }


    internal class ArgDefinition
    {
        public string argName;
        public string argDescription;
        public List<int> argAttributes;
        public readonly bool usableMultibleTimes;
        public readonly bool undocumented;


        public ArgDefinition(string argName, string argDescription, List<int> argAttributes, bool usableMultibleTimes, bool undocumented)
        {
            this.usableMultibleTimes = usableMultibleTimes;
            this.argName = argName;
            this.argDescription = argDescription;
            this.argAttributes = argAttributes;
            this.undocumented = undocumented;
        }
    }
}
