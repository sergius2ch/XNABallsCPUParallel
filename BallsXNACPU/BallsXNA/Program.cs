using System;

namespace BallsXNA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            /*OptionsForm form = new OptionsForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)*/
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
}

