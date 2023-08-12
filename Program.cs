using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supdate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0) // Startup mode
            {


                return;
            }
            ArgCheck.InterpretArguments(ArgCheck.TokeniseArgs(args, ArgCheck.argCommandsDefinitions));
        }
    }
}
