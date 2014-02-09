using System;

namespace DogePong
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DogePong game = new DogePong())
            {
                game.Run();
            }
        }
    }
#endif
}

