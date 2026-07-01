using System.IO.Compression;
using sayclip;

internal static class Program
{
    private static int Main(string[] args)
    {
        string? pluginFolderName = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(pluginFolderName))
        {
            pluginFolderName = "gTranslateBingTranslator";
        }

        string? repoRoot = ResolveRepositoryRoot();
        if (repoRoot is null)
        {
            Console.Error.WriteLine("ERROR | Cannot resolve repository root from current execution path.");
            return 2;
        }

        string sourcePluginFolder = Path.Combine(repoRoot, "sayclip", "sayclip", "plugins", pluginFolderName);
        if (!Directory.Exists(sourcePluginFolder))
        {
            Console.Error.WriteLine($"ERROR | Source plugin folder not found: {sourcePluginFolder}");
            return 3;
        }

        string runtimeRoot = AppContext.BaseDirectory;
        string packageName = pluginFolderName + "-plugin.package";
        string packagePath = Path.Combine(runtimeRoot, packageName);
        string pluginsTargetFolder = Path.Combine(runtimeRoot, "plugins");

        try
        {
            CleanupFile(packagePath);
            CleanupDirectory(pluginsTargetFolder);
            CleanupDirectory(Path.Combine(runtimeRoot, "plugins-backup"));
            CleanupDirectory(Path.Combine(runtimeRoot, "unpack-tmp"));

            Directory.CreateDirectory(pluginsTargetFolder);
            CreatePluginPackage(sourcePluginFolder, packagePath);

            ConfigurationManager.getInstance.unpackNewPlugins = true;
            ConfigurationManager.getInstance.deletePluginsPackagesAfterInstall = false;

            PluginManager manager = PluginManager.getInstanse;
            List<string> plugins = manager.getPluginsNames();

            bool pluginLoaded = plugins.Any(name =>
                name.IndexOf("bing", StringComparison.OrdinalIgnoreCase) >= 0 &&
                name.IndexOf("gtranslate", StringComparison.OrdinalIgnoreCase) >= 0);

            bool packageRenamed = File.Exists(packagePath + ".installed");
            bool pluginDllInstalled = Directory.GetFiles(pluginsTargetFolder, "*.scplug.dll", SearchOption.AllDirectories).Length > 0;

            Console.WriteLine($"PACKAGE_PATH | {packagePath}");
            Console.WriteLine($"PLUGIN_FOLDER_SOURCE | {sourcePluginFolder}");
            Console.WriteLine($"PLUGIN_DLL_INSTALLED | {pluginDllInstalled}");
            Console.WriteLine($"PACKAGE_RENAMED | {packageRenamed}");
            Console.WriteLine($"PLUGINS_LOADED_COUNT | {plugins.Count}");
            foreach (string name in plugins)
            {
                Console.WriteLine($"PLUGIN | {name}");
            }

            if (!pluginDllInstalled)
            {
                Console.Error.WriteLine("ERROR | No plugin DLL installed from package.");
                return 4;
            }

            if (!packageRenamed)
            {
                Console.Error.WriteLine("ERROR | Package was not renamed as installed.");
                return 5;
            }

            if (!pluginLoaded)
            {
                Console.Error.WriteLine("ERROR | Expected bing plugin was not loaded by PluginManager.");
                return 6;
            }

            Console.WriteLine("SUMMARY | status=OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR | {ex}");
            return 10;
        }
    }

    private static void CreatePluginPackage(string sourcePluginFolder, string packagePath)
    {
        using ZipArchive archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);

        foreach (string file in Directory.GetFiles(sourcePluginFolder, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourcePluginFolder, file);
            archive.CreateEntryFromFile(file, relativePath);
        }
    }

    private static string? ResolveRepositoryRoot()
    {
        string[] candidates = new[]
        {
            Environment.CurrentDirectory,
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."))
        };

        foreach (string candidate in candidates)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(candidate);
            }
            catch
            {
                continue;
            }

            if (Directory.Exists(Path.Combine(fullPath, "sayclip", "sayclip")))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static void CleanupDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void CleanupFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
