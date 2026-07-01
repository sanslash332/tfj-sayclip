using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using sayclip;

internal static class Program
{
    private sealed class ProbeResult
    {
        public string AssemblyPath { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public string Status { get; set; } = "FAIL";
        public bool InitializeOk { get; set; }
        public int LanguageCount { get; set; } = -1;
        public string TranslateStatus { get; set; } = "SKIPPED";
        public string Error { get; set; } = string.Empty;
    }

    private static int Main(string[] args)
    {
        bool strict = args.Any(a => string.Equals(a, "--strict", StringComparison.OrdinalIgnoreCase));
        bool noTranslateSmoke = args.Any(a => string.Equals(a, "--no-translate-smoke", StringComparison.OrdinalIgnoreCase));
        string? pluginsRootArg = ParseOption(args, "--plugins-root");
        string? pluginsRoot = ResolvePluginRoot(pluginsRootArg);

        if (pluginsRoot is null)
        {
            Console.Error.WriteLine("ERROR | plugins folder not found. Use --plugins-root <path>.");
            return 2;
        }

        var pluginAssemblies = Directory.GetFiles(pluginsRoot, "*.scplug.dll", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (pluginAssemblies.Count == 0)
        {
            Console.Error.WriteLine($"ERROR | no .scplug.dll files found in {pluginsRoot}");
            return 3;
        }

        var searchDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            pluginsRoot
        };

        foreach (string asm in pluginAssemblies)
        {
            string? dir = Path.GetDirectoryName(asm);
            if (!string.IsNullOrEmpty(dir))
            {
                searchDirs.Add(dir);
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) =>
        {
            string requestedName = new AssemblyName(eventArgs.Name).Name + ".dll";
            foreach (string dir in searchDirs)
            {
                string candidate = Path.Combine(dir, requestedName);
                if (!File.Exists(candidate))
                {
                    continue;
                }

                try
                {
                    return Assembly.LoadFrom(candidate);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        };

        var results = new List<ProbeResult>();

        foreach (string pluginAssembly in pluginAssemblies)
        {
            results.Add(ProbeAssembly(pluginAssembly, noTranslateSmoke));
        }

        int okCount = results.Count(r => r.Status == "OK");
        int skipCount = results.Count(r => r.Status == "SKIP_PLATFORM");
        int failCount = results.Count(r => r.Status == "FAIL");

        Console.WriteLine($"PROBE_ROOT | {pluginsRoot}");
        Console.WriteLine($"PROBE_TOTAL | {results.Count}");

        foreach (ProbeResult r in results)
        {
            string asmName = Path.GetFileName(r.AssemblyPath);
            if (r.Status == "OK")
            {
                Console.WriteLine($"OK | {asmName} | name={r.PluginName} | init={r.InitializeOk} | langs={r.LanguageCount} | translate={r.TranslateStatus}");
            }
            else if (r.Status == "SKIP_PLATFORM")
            {
                Console.WriteLine($"SKIP_PLATFORM | {asmName} | reason={r.Error}");
            }
            else
            {
                Console.WriteLine($"FAIL | {asmName} | name={r.PluginName} | init={r.InitializeOk} | langs={r.LanguageCount} | translate={r.TranslateStatus} | error={r.Error}");
            }
        }

        Console.WriteLine($"SUMMARY | ok={okCount} | skip_platform={skipCount} | fail={failCount} | strict={strict}");

        if (strict)
        {
            return (failCount == 0 && skipCount == 0) ? 0 : 1;
        }

        return failCount == 0 ? 0 : 1;
    }

    private static ProbeResult ProbeAssembly(string pluginAssemblyPath, bool noTranslateSmoke)
    {
        var result = new ProbeResult
        {
            AssemblyPath = pluginAssemblyPath
        };

        iSayclipPluginTranslator? translator = null;
        CompositionContainer? container = null;

        try
        {
            string pluginDir = Path.GetDirectoryName(pluginAssemblyPath) ?? string.Empty;
            string pluginFile = Path.GetFileName(pluginAssemblyPath);

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(pluginDir, pluginFile));
            container = new CompositionContainer(catalog);
            translator = container.GetExports<iSayclipPluginTranslator>().Select(e => e.Value).FirstOrDefault();

            if (translator is null)
            {
                result.Status = "FAIL";
                result.Error = "No MEF export for iSayclipPluginTranslator";
                return result;
            }

            result.PluginName = SafeGetName(translator);
            result.InitializeOk = translator.initialize(null);

            var langs = translator.getAvailableLanguages("en").GetAwaiter().GetResult();
            result.LanguageCount = langs?.Count() ?? 0;

            if (noTranslateSmoke)
            {
                result.TranslateStatus = "SKIPPED_BY_FLAG";
            }
            else
            {
                try
                {
                    string translated = translator.translate("hello world").GetAwaiter().GetResult();
                    result.TranslateStatus = string.IsNullOrWhiteSpace(translated) ? "EMPTY" : "OK";
                }
                catch (Exception tex)
                {
                    result.TranslateStatus = $"FAIL ({tex.GetType().Name}: {tex.Message})";
                }
            }

            result.Status = "OK";
            return result;
        }
        catch (ReflectionTypeLoadException rtle) when (IsPlatformDependencyIssue(rtle))
        {
            result.Status = "SKIP_PLATFORM";
            result.Error = ExtractLoaderErrors(rtle);
            return result;
        }
        catch (Exception ex)
        {
            result.Status = "FAIL";
            result.Error = ex.ToString().Replace(Environment.NewLine, " ");
            return result;
        }
        finally
        {
            container?.Dispose();
        }
    }

    private static bool IsPlatformDependencyIssue(ReflectionTypeLoadException ex)
    {
        if (OperatingSystem.IsWindows())
        {
            return false;
        }

        string loaderErrors = ExtractLoaderErrors(ex);
        return loaderErrors.Contains("PresentationFramework", StringComparison.OrdinalIgnoreCase)
            || loaderErrors.Contains("WindowsBase", StringComparison.OrdinalIgnoreCase)
            || loaderErrors.Contains("Microsoft.WindowsDesktop.App", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractLoaderErrors(ReflectionTypeLoadException ex)
    {
        var messages = ex.LoaderExceptions
            .Where(e => e is not null)
            .Select(e => e!.Message)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join(" | ", messages);
    }

    private static string SafeGetName(iSayclipPluginTranslator translator)
    {
        try
        {
            return translator.getName();
        }
        catch
        {
            return "<name-error>";
        }
    }

    private static string? ResolvePluginRoot(string? explicitPath)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            candidates.Add(explicitPath);
        }

        string cwd = Environment.CurrentDirectory;
        candidates.Add(Path.Combine(cwd, "sayclip", "sayclip", "plugins"));
        candidates.Add(Path.Combine(cwd, "sayclip", "plugins"));
        candidates.Add(Path.Combine(cwd, "plugins"));

        string baseDir = AppContext.BaseDirectory;
        candidates.Add(Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "sayclip", "plugins")));
        candidates.Add(Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "sayclip", "sayclip", "plugins")));

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

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string? ParseOption(string[] args, string optionName)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }

            return null;
        }

        return null;
    }
}
