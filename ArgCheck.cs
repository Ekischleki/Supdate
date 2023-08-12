using System.Diagnostics;

namespace Supdate
{
    internal class ArgCheck
    {
        public static readonly List<ArgDefinition> argCommandsDefinitions = new()
        {
            new("r", "replace arg one with arg two and then start arg 2 with the intend to delete this file", new() {2}, false ),
            new("d", "delete arg 1", new() {1}, false),


        };
        static void StartProgram(string path)
        {
            // Create a new ProcessStartInfo to configure how the process will start
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,            // The path to the executable
                UseShellExecute = true,     // Use the shell to execute (allows things like file associations)
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false      // Whether to create a separate window for the process
            };

            try
            {
                // Start the process
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                ConsoleLog.Fatality("An error occurred starting a program: " + ex.Message);
                
            }
        }
        public static void InterpretArguments(List<ArgInstance> tokens)
        {
            foreach (ArgInstance token in tokens)
            {
                switch (token.argDefinition.argName)
                {
                    case "r":
                        for (int i = 0; i < 10; i++)
                            try
                            {
                                ConsoleLog.Log($"Trying to delete arg 1 try {i} of 10");
                                File.Delete(token.argAttributes[0]);
                            } catch(Exception ex)
                            {
                                ConsoleLog.Error($"Failed to delete arg 1 because {ex.Message}");
                                Thread.Sleep(500);

                            }
                        for (int i = 0; i < 10; i++)
                            try
                            {
                                ConsoleLog.Log($"Trying to move arg 2 to arg 1 try {i} of 10");
                                File.Move(token.argAttributes[1], token.argAttributes[0]);

                            }
                            catch (Exception ex)
                            {
                                ConsoleLog.Error($"Failed to move because {ex.Message}");
                                Thread.Sleep(500);
                            }
                        StartProgram(token.argAttributes[0]);
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


        public ArgDefinition(string argName, string argDescription, List<int> argAttributes, bool usableMultibleTimes)
        {
            this.usableMultibleTimes = usableMultibleTimes;
            this.argName = argName;
            this.argDescription = argDescription;
            this.argAttributes = argAttributes;
        }
    }
}
