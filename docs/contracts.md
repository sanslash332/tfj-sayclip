# Documentación de Contratos (Interfaces)

## Descripción General

El proyecto `sayclip.contracts` define las interfaces que todo plugin de traducción debe implementar. Estas interfaces establecen el contrato entre el core y los plugins, permitiendo crear nuevos traductores sin dependencia directa del core.

## Interfaces Principales

### 1. **iSayclipPluginTranslator** - Contrato Principal

Interfaz que todo traductor debe implementar. Define el comportamiento mínimo requerido.

```csharp
public interface iSayclipPluginTranslator
{
    string getName();
    string getDescription(string languaje);
    Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje);
    void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang);
    SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje);
    bool initialize(ISayclipPluginContext context = null);
    Task<string> translate(string text);
    bool haveConfigWindow();
    void showConfigWindow(string displayLanguaje);
}
```

#### Métodos Detallados

**`string getName()`**
- **Propósito**: Devuelve nombre identificador único del plugin
- **Retorno**: Nombre único (ej: "gTranslate bing translator plugin")
- **Uso**: Identificar plugin en UI, guardar en configuración

**`string getDescription(string languaje)`**
- **Propósito**: Devuelve descripción legible del plugin
- **Parámetro**: Idioma para la descripción
- **Retorno**: Texto descriptivo
- **Uso**: Mostrar en UI información sobre el traductor

**`Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)`**
- **Propósito**: Obtiene lista de idiomas soportados por el traductor
- **Parámetro**: Idioma en que mostrar los nombres de idiomas
- **Retorno**: Tarea async con lista de `SayclipLanguage`
- **Uso**: Llenar ComboBoxes de selección de idiomas
- **Nota**: Debe ser async porque puede hacer peticiones HTTP

**`void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)`**
- **Propósito**: Establece idioma origen y destino seleccionados
- **Parámetros**: Idioma origen y destino como objetos `SayclipLanguage`
- **Uso**: Configurar traductor antes de traducir
- **Importante**: Debe ser llamado antes de `translate()`

**`SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)`**
- **Propósito**: Devuelve los idiomas actualmente configurados
- **Parámetro**: Idioma para mostrar nombres
- **Retorno**: Array con [idioma_origen, idioma_destino]
- **Uso**: Sincronizar UI con estado actual del plugin

**`bool initialize(ISayclipPluginContext context = null)`**
- **Propósito**: Inicializa el plugin después de cargarse
- **Parámetro**: Contexto con servicios de logging y accesibilidad
- **Retorno**: true si inicialización fue exitosa
- **Uso**: Cargar configuración guardada, validar dependencias
- **Nota**: Llamado automáticamente por PluginManager después de instanciar

**`Task<string> translate(string text)`**
- **Propósito**: Traduce un texto del idioma origen al destino
- **Parámetro**: Texto a traducir
- **Retorno**: Tarea async con texto traducido
- **Uso**: Flujo principal de traducción
- **Importante**: 
  - Debe ser async (puede hacer peticiones HTTP)
  - Debe lanzar excepción si falla
  - Debe validar que idiomas estén configurados

**`bool haveConfigWindow()`**
- **Propósito**: Indica si el plugin tiene ventana de configuración
- **Retorno**: true si hay UI de configuración, false si no
- **Uso**: Habilitar/deshabilitar botón "Config" en UI
- **Nota**: No todos los plugins necesitan configuración

**`void showConfigWindow(string displayLanguaje)`**
- **Propósito**: Abre ventana modal para configurar el plugin
- **Parámetro**: Idioma en que mostrar la ventana de configuración
- **Uso**: Permitir usuario configurar parámetros específicos del traductor
- **Nota**: Solo se llama si `haveConfigWindow()` retorna true

### 2. **ISayclipPluginContext** - Contexto del Plugin

Proporciona servicios compartidos que el plugin puede utilizar.

```csharp
public interface ISayclipPluginContext
{
    ISayclipAccessibility Accessibility { get; }
    ISayclipLogger Logger { get; }
}
```

#### Propiedades

**`ISayclipAccessibility Accessibility`**
- Proporciona acceso a servicios de accesibilidad (lectores de pantalla)
- Permite al plugin reproducir audio si es necesario

**`ISayclipLogger Logger`**
- Proporciona acceso al sistema de logging centralizado
- Permite registrar eventos, advertencias y errores

### 3. **ISayclipAccessibility** - Interfaz de Accesibilidad

Define métodos para interactuar con lectores de pantalla.

#### Métodos Típicos

- `Speech(string text, bool interrupt)` - Reproduce texto en voz alta
- Soporte para múltiples lectores (NVDA, JAWS, Narrador)

### 4. **ISayclipLogger** - Interfaz de Logging

Define métodos para logging centralizado.

#### Métodos Típicos

- `Debug(string message)` - Registra mensaje de debug
- `Info(string message)` - Registra información
- `Warn(string message)` - Registra advertencia
- `Error(string message, Exception ex)` - Registra error con excepción

### 5. **SayclipLanguage** - Modelo de Idioma

Representa un idioma soportado.

```csharp
public class SayclipLanguage
{
    public string langName { get; set; }        // Nombre (ej: "English")
    public string langCode { get; set; }        // Código (ej: "en")
    public bool isAutoDetect { get; set; }      // Es auto-detección
    public bool isDeprecated { get; set; }      // Está deprecado
}
```

## Dependencias de Contratos

```
iSayclipPluginTranslator
  ├─ SayclipLanguage (modelo de idioma)
  └─ ISayclipPluginContext
      ├─ ISayclipAccessibility
      └─ ISayclipLogger
```

## Patrones de Implementación

### Patrón 1: Plugin Simple (sin HTTP)

```csharp
public class SimpleTranslator : iSayclipPluginTranslator
{
    private SayclipLanguage fromLang, toLang;
    private ISayclipPluginContext _context;

    public bool initialize(ISayclipPluginContext context = null)
    {
        _context = context;
        _context?.Logger?.Info($"{getName()} initialized");
        return true;
    }

    public Task<string> translate(string text)
    {
        // Sin HTTP, retorna resultado simple
        return Task.FromResult($"Translated: {text}");
    }

    public bool haveConfigWindow() => false;
    // ... otras implementaciones
}
```

### Patrón 2: Plugin con HTTP (GTranslate)

```csharp
public class GTranslateTranslator : iSayclipPluginTranslator
{
    private BingTranslator translator;
    
    public bool initialize(ISayclipPluginContext context = null)
    {
        translator = new BingTranslator();
        return true;
    }

    public async Task<string> translate(string text)
    {
        // HTTP request en background
        var result = await translator.TranslateAsync(text, toLang, fromLang);
        return result.Translation;
    }
}
```

### Patrón 3: Plugin con Configuración

```csharp
public class ConfigurableTranslator : iSayclipPluginTranslator
{
    public bool haveConfigWindow() => true;
    
    public void showConfigWindow(string displayLanguaje)
    {
        var configWindow = new ConfigWindow();
        configWindow.Owner = App.Current.MainWindow;
        configWindow.ShowDialog();
    }
}
```

## MEF y Atributo Export

Los plugins deben usar atributo `[Export]` para ser descubiertos por MEF:

```csharp
[Export(typeof(sayclip.iSayclipPluginTranslator))]
public class Translator : iSayclipPluginTranslator
{
    // Implementación
}
```

Este atributo permite a `PluginManager` descubrir automáticamente el plugin.

## Ciclo de Vida del Plugin

```
1. [MEF Discovery] PluginManager busca *.scplug.dll en plugins/
   ↓
2. [Instantiation] Se crea instancia del plugin
   ↓
3. [Initialization] Se llama initialize(context)
   ↓
4. [Configuration] Usuario configura idiomas via setLanguages()
   ↓
5. [Translation] Se llama translate() repetidamente
   ↓
6. [Config Window] Opcionalmente showConfigWindow() si haveConfigWindow()
   ↓
7. [Shutdown] Objeto se descarta
```

## Flujo de Traducción Típico

```csharp
// 1. Core obtiene plugin
iSayclipPluginTranslator plugin = PluginManager.getActivePlugin;

// 2. Core inicializa (si es nuevo)
plugin.initialize(context);

// 3. Core obtiene idiomas disponibles
var languages = await plugin.getAvailableLanguages("es");

// 4. Core configura idiomas
plugin.setLanguages(fromLang, toLang);

// 5. Core traduce
string result = await plugin.translate("Hello world");
// Result: "Hola mundo"
```

## Convenciones de Implementación

1. **Nombres**: Usar convención de nomenclatura consistente
2. **Excepciones**: Lanzar excepciones legibles si algo falla
3. **Async**: Métodos HTTP deben ser async/await
4. **Logging**: Usar `_context?.Logger?` para logging optional
5. **Configuración**: Guardar/restaurar en `Properties.Settings` si es persistente
6. **Idiomas**: Incluir "auto-detect" si el servicio lo soporta

## Puntos Clave para Implementadores

- Siempre implementar todos los métodos
- El método `translate()` DEBE ser thread-safe
- Usar `[Export]` obligatoriamente para descubrimiento
- Manejo de excepciones adecuado en `translate()`
- NO bloquear el thread con operaciones sincrónicas (usar async)
- NO asumir orden de llamada de métodos (puede variar)

---

**Ubicación**: `sayclip/sayclip.contracts/`  
**Dependencias**: Ninguna (aislado del core)  
**Uso**: Base para crear nuevos plugins independientes
