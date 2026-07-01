# Guía: Cómo Crear tu Propio Plugin

## Introducción

Esta guía te lleva paso a paso para crear un plugin de traducción para sayclip. Un plugin es un componente independiente que implementa un servicio de traducción y se integra dinámicamente al core.

## Requisitos Previos

- Visual Studio 2022 o superior (o VS Code con dotnet CLI)
- .NET 9.0 SDK
- Referencia a `sayclip.contracts` proyecto (interfaces)
- Entender conceptos básicos de async/await en C#

## Paso 1: Crear Proyecto

### Opción A: Desde Visual Studio

1. Crear nuevo proyecto: **Class Library (.NET)**
   - Nombre: `{NombreTutraductor}TranslatorPlugin`
   - Framework: .NET 9.0-windows
   - Ubicación: En carpeta `sayclip/` (junto a otros plugins)

2. Estructura de carpetas:
```
{NombreTutraductor}TranslatorPlugin/
├─ {NombreTuductor}TranslatorPlugin.csproj
├─ Translator.cs (clase principal)
├─ Properties/
│  └─ Settings.settings (si necesitas persistencia)
└─ bin/obj (generados automáticamente)
```

### Opción B: Desde Terminal

```bash
cd sayclip
dotnet new classlib -n {NombreTuductor}TranslatorPlugin -f net9.0-windows
```

## Paso 2: Agregar Referencias

### En {NombreTuductor}TranslatorPlugin.csproj

Añade referencia a contratos:

```xml
<ItemGroup>
  <ProjectReference Include="../sayclip.contracts/sayclip.contracts.csproj" />
</ItemGroup>
```

### NuGet Packages (si necesitas)

```xml
<ItemGroup>
  <PackageReference Include="System.ComponentModel.Composition" Version="9.0.0" />
  <!-- Agregar librerías del traductor externo si las necesitas -->
</ItemGroup>
```

## Paso 3: Crear Clase Principal

Archivo: `Translator.cs`

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using sayclip;

namespace {NombreTuductor}TranslatorPlugin
{
    [Export(typeof(iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string name = "{Nombre Legible} Translator Plugin";
        private const string description = "Traductor usando {Servicio}";
        
        private string fromLang = "en";
        private string toLang = "es";
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private ISayclipPluginContext _context;

        // Implementaciones irán aquí
    }
}
```

### Explicación de Decorador

`[Export(typeof(iSayclipPluginTranslator))]` le indica a MEF que:
- Esta clase implementa la interfaz
- Debe ser descubierta automáticamente
- Puede ser inyectada donde se necesite

## Paso 4: Implementar Interfaz

### Métodos Básicos (Información)

```csharp
public string getName()
{
    return name;
}

public string getDescription(string languaje)
{
    return description;
}

public bool haveConfigWindow()
{
    // Retorna true si tienes ventana XAML de configuración
    return false;
}

public void showConfigWindow(string displayLanguaje)
{
    // Si haveConfigWindow() es false, este método no se llama
    throw new NotImplementedException();
}
```

### Métodos de Idiomas

```csharp
public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
{
    // Retorna lista de idiomas soportados
    var languages = new List<SayclipLanguage>();
    
    // Agregar auto-detección (si el servicio lo soporta)
    languages.Add(new SayclipLanguage("Auto-detect", "auto", true, false));
    
    // Agregar idiomas específicos
    languages.Add(new SayclipLanguage("English", "en", false, false));
    languages.Add(new SayclipLanguage("Spanish", "es", false, false));
    languages.Add(new SayclipLanguage("French", "fr", false, false));
    
    return languages;
}

public void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)
{
    // Guarda los idiomas seleccionados
    this.fromLang = fromLang.langCode;
    this.toLang = toLang.langCode;
    this.fromLangSayclip = fromLang;
    this.toLangSayclip = toLang;
    
    // Aquí puedes guardar en Settings si es persistente
    // Properties.Settings.Default.fromLang = this.fromLang;
    // Properties.Settings.Default.Save();
}

public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
{
    // Retorna idiomas actualmente configurados
    return new SayclipLanguage[]
    {
        this.fromLangSayclip,
        this.toLangSayclip
    };
}
```

### Inicialización

```csharp
public bool initialize(ISayclipPluginContext context = null)
{
    // Guardar contexto para usar logging/accesibilidad
    _context = context;
    
    // Registrar inicialización
    _context?.Logger?.Info($"{getName()} initializado");
    
    // Validar dependencias, cargar configuración guardada, etc.
    try
    {
        // Tu lógica de inicialización aquí
        return true;
    }
    catch (Exception ex)
    {
        _context?.Logger?.Error($"Error inicializando {getName()}", ex);
        return false;
    }
}
```

### Traducción (Método Principal)

```csharp
public async Task<string> translate(string text)
{
    // Validar entrada
    if (string.IsNullOrWhiteSpace(text))
    {
        throw new ArgumentException("Text cannot be empty");
    }
    
    // Validar que idiomas estén configurados
    if (string.IsNullOrEmpty(fromLang) || string.IsNullOrEmpty(toLang))
    {
        throw new InvalidOperationException("Languages not configured");
    }
    
    try
    {
        _context?.Logger?.Debug($"Translating: {text}");
        
        // AQUÍ VA TU LÓGICA DE TRADUCCIÓN
        // Ejemplo simple (reemplaza con tu implementación):
        string result = await TranslateViaService(text, fromLang, toLang);
        
        _context?.Logger?.Debug($"Result: {result}");
        return result;
    }
    catch (Exception ex)
    {
        _context?.Logger?.Error($"Translation error: {ex.Message}", ex);
        throw;  // Es importante lanzar excepción si falla
    }
}

private async Task<string> TranslateViaService(string text, string from, string to)
{
    // Implementación específica de tu traductor
    // Por ejemplo, si usas HTTP:
    
    using (var client = new HttpClient())
    {
        // Construir petición
        string url = $"https://api-traductor.com/v1/translate?text={text}&from={from}&to={to}";
        
        // Hacer petición async
        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API error: {response.StatusCode}");
        }
        
        // Parsear respuesta
        var content = await response.Content.ReadAsStringAsync();
        // Extraer traducción del JSON/XML
        string translation = ExtractTranslation(content);
        
        return translation;
    }
}

private string ExtractTranslation(string response)
{
    // Parsear y extraer texto traducido de la respuesta
    // Implementación específica de tu API
    return response; // Simplificado
}
```

## Paso 5: Configurar Propiedades del Proyecto

### En {NombreTuductor}TranslatorPlugin.csproj

```xml
<PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Platforms>AnyCPU;x86</Platforms>
    <AssemblyName>{NombreTuductor}TranslatorPlugin.scplug</AssemblyName>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <OutputPath>..\..\plugins\{NombreTuductor}Translator\</OutputPath>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <OutputPath>..\..\plugins\{NombreTuductor}Translator\</OutputPath>
</PropertyGroup>
```

**Puntos clave**:
- `AssemblyName` termina en `.scplug` para identificarlo como plugin
- `OutputPath` apunta a carpeta dentro de `plugins/`
- `CopyLocalLockFileAssemblies` copia dependencias necesarias

## Paso 6: Compilar y Probar

### Compilación

```bash
cd sayclip
dotnet build {NombreTuductor}TranslatorPlugin -p:Platform=x86
```

El DLL generado irá a: `plugins/{NombreTuductor}Translator/`

### Testing con PluginRuntimeProbe

```bash
dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug
```

Validará automáticamente tu plugin.

## Paso 7: Agregar a Solución (Opcional)

Para incluir en compilación de solución:

```bash
dotnet sln sayclip.sln add {NombreTuductor}TranslatorPlugin/{NombreTuductor}TranslatorPlugin.csproj
```

## Paso 8: Empaquetar Plugin (Opcional)

Para distribuir como paquete:

```bash
# Crear ZIP con plugin compilado
cd plugins/{NombreTuductor}Translator
zip {NombreTuductor}-plugin.package {NombreTuductor}TranslatorPlugin.scplug.dll GTranslate.dll ...
```

Usuarios pueden copiar `.package` a carpeta raíz y sayclip lo instalará automáticamente.

## Checklist Completo

- Proyecto creado (.NET 9.0-windows)
- Referencia a `sayclip.contracts` agregada
- Clase hereda de `iSayclipPluginTranslator`
- Atributo `[Export(...)]` presente
- `getName()` implementado
- `getDescription()` implementado
- `getAvailableLanguages()` implementado
- `setLanguages()` implementado
- `getConfiguredLanguajes()` implementado
- `initialize()` implementado
- `translate()` implementado (método principal)
- `haveConfigWindow()` implementado
- `showConfigWindow()` implementado (si necesario)
- AssemblyName termina en `.scplug`
- OutputPath apunta a carpeta plugins/
- Compilación exitosa sin errores
- PluginRuntimeProbe valida plugin
- Probado manualmente en sayclip

## Ejemplo Completo Mínimo

```csharp
[Export(typeof(iSayclipPluginTranslator))]
public class Translator : iSayclipPluginTranslator
{
    private string fromLang = "en";
    private string toLang = "es";
    private SayclipLanguage[] langs;

    public bool initialize(ISayclipPluginContext context = null) => true;
    
    public string getName() => "Mi Traductor";
    
    public string getDescription(string lang) => "Mi traductor personalizado";
    
    public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string lang)
    {
        return new[]
        {
            new SayclipLanguage("English", "en"),
            new SayclipLanguage("Spanish", "es"),
        };
    }
    
    public void setLanguages(SayclipLanguage from, SayclipLanguage to)
    {
        fromLang = from.langCode;
        toLang = to.langCode;
    }
    
    public SayclipLanguage[] getConfiguredLanguajes(string lang) 
        => new[] { new SayclipLanguage("English", fromLang), new SayclipLanguage("Spanish", toLang) };
    
    public async Task<string> translate(string text)
    {
        // Implementar traducción aquí
        return await Task.FromResult($"[{toLang}]: {text}");
    }
    
    public bool haveConfigWindow() => false;
    
    public void showConfigWindow(string lang) { }
}
```

## Troubleshooting

### "Plugin no aparece en lista"

- Verifica DLL termina en `.scplug.dll`
- Verifica carpeta destino es `plugins/{NombrePlugin}/`
- Verifica atributo `[Export(typeof(iSayclipPluginTranslator))]`
- Recarga aplicación (PluginManager es Lazy)

### "Error al traducir"

- Verifica `initialize()` fue llamado exitosamente
- Verifica `setLanguages()` fue llamado antes de `translate()`
- Chequea logs con `_context?.Logger?.Error()`

### "Falta referencia"

- Verifica ProjectReference a `sayclip.contracts`
- Verifica CopyLocalLockFileAssemblies=true
- Verifica todas las DLLs de dependencias están en carpeta plugins/

---

**Documentación relacionada**: [Plugin System](./plugins.md) | [Contracts](./contracts.md)  
**Ejemplo en repo**: `sayclip/gTranslateBingTranslatorPlugin/`
