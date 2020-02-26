using System;
using System.Collections.Generic;
using System.Text;

namespace NightscoutLightsService.Helpers
{
   public static class ConsoleHelper
    {

        public static void Write(string msg)
        {

            Console.WriteLine($"NightScout --> {msg} at {DateTime.Now.ToShortTimeString()}");

        }


    }
}
