using System;

namespace LD49
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new LD49Game())
                game.Run();
        }
    }
}
