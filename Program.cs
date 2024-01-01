using System;
using System.Configuration;
using  WPF = System.Windows;
using System.Data.Entity;

namespace DMGINC
{
   
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the System.Windows.Application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize System.Windows.Application configuration such as set high DPI settings or default font,
            // see https://aka.ms/System.Windows.Applicationconfiguration.
            windowMain form = new windowMain();
            System.Windows.Application app = new System.Windows.Application();
            app.Run(form);
            


        }
    }
}