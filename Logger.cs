namespace Supdate
{
    internal class ConsoleLog
    {
        public static void Warn(object message) 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}]: {message}");
            Console.ResetColor();
        }
        public static void Error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}]: {message}");
            Console.ResetColor();
        }
        public static void Log(object message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now}]: {message}");
            Console.ResetColor();
        }
        public static void Fatality(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[{DateTime.Now}] 💀Fatal error: {message}");
            Console.ResetColor();
        }
    }
}