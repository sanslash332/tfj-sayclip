using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace sayclipTray
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
            {
                string path = Path.Combine(AppContext.BaseDirectory, "lib", assemblyName.Name + ".dll");
                if (File.Exists(path))
                    return context.LoadFromAssemblyPath(path);
                return null;
            };

            RunApp(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void RunApp(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
