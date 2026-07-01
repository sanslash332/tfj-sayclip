# Documentación del Sistema de Logging

## Descripción General

El sistema de logging de sayclip (proyecto `logSystem`) centraliza todas las operaciones de registro de eventos, errores y información de depuración. Utiliza **NLog** como librería de logging, proporcionando un sistema flexible y configurable para capturar actividad en archivos de log.

## Componentes Principales

### 1. **LogWriter.cs** - Fachada Singleton

Punto central de acceso al sistema de logging. Implementa patrón Singleton para garantizar una única instancia del logger.

```csharp
public static Logger getLog()
{
    // Retorna instancia única del logger NLog
}
```

#### Métodos Principales

**`static Logger getLog()`**
- Propósito: Obtener instancia única del logger
- Retorno: Objeto Logger de NLog listo para usar
- Uso: Llamada inicial para registrar eventos
- Patrón: Lazy initialization - inicializa solo al primer acceso

**`static void init()` (privado)**
- Propósito: Inicializar el sistema de logging
- Llamado automáticamente por `getLog()` en primer acceso
- Inicializa ConfigurationManager de NLog
- Registra evento de inicio del sistema

#### Características

- Thread-safe mediante objeto lock interno
- Instancia estática para acceso global
- Inicialización lazy (solo cuando se necesita)
- Registro automático del timestamp de inicio

### 2. **NLog.config** - Archivo de Configuración

Archivo XML que define cómo se comporta NLog. Especifica dónde se guardan logs, qué información se captura y cómo se formatea.

#### Estructura Principal

```xml
<nlog>
  <!-- Configuración global -->
  <targets>
    <!-- Define destinos (archivos, consola, etc.) -->
  </targets>
  
  <rules>
    <!-- Define qué eventos van a qué destinos -->
  </rules>
</nlog>
```

#### Target Configurado

**Target de Archivo**
- Tipo: `File`
- Ubicación: `{AppRoot}/logs/sayclip {fecha}.log`
- Nombre: archivo dinámico con fecha actual
- Ejemplo: `logs/sayclip 2026-06-30.log`

#### Formato de Log

```
${longdate} ${uppercase:${level}} ${message}
```

Ejemplo de línea de log:
```
2026-06-30 14:35:42.1234 DEBUG Starting sayclip core
2026-06-30 14:35:43.5678 INFO gTranslate bing translator plugin seted as active plugin
2026-06-30 14:35:44.9012 WARN no active plugin detected
2026-06-30 14:35:45.3456 ERROR Error setting text on the clipboard
```

#### Niveles de Log Configurados

Todas las reglas usan `minlevel="Debug"`, lo que significa captura:
- `DEBUG` - Información de depuración detallada
- `INFO` - Información general
- `WARN` - Advertencias
- `ERROR` - Errores
- `FATAL` - Errores críticos

### 3. **NLog.xsd** - Schema de Validación

Archivo de esquema XML que define la estructura válida de NLog.config. Usado por editores para validación y autocompletado.

## Integración en el Sistema

### Cómo se Usa en el Core

En `Sayclip.cs`:
```csharp
LogWriter.getLog().Debug("Starting sayclip core");
LogWriter.getLog().Info("Sayclip using " + translator.getName());
LogWriter.getLog().Warn("no active plugin detected");
LogWriter.getLog().Error("Error setting text on the clipboard", exception);
```

En `PluginManager.cs`:
```csharp
LogWriter.getLog().Debug($"the saved configuration value is: {Properties.Settings.Default.translator}");
LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");
LogWriter.getLog().Warn("no active plugin detected");
```

En `ScreenReaderControl.cs`:
```csharp
LogWriter.getLog().Error("Error reading from screen reader", ex);
```

### Flujo de Logging

```
1. Código llama: LogWriter.getLog().Info("mensaje")
   
2. LogWriter retorna instancia de Logger (inicializa si es primera vez)
   
3. Logger (NLog) evalúa nivel configurado
   
4. Si el nivel cumple minlevel="Debug":
   ├─ Aplica layout: "${longdate} ${uppercase:${level}} ${message}"
   └─ Escribe a archivo de destino
   
5. Archivo: logs/sayclip {fecha-actual}.log
   
6. El archivo se crea automáticamente si no existe
```

## Ubicación de Logs

Logs se guardan en:
```
{DirectorioInstalacion}/logs/
├─ sayclip 2026-06-30.log
├─ sayclip 2026-06-29.log
└─ ...
```

Un archivo nuevo se crea cada día, facilitando revisión histórica.

## Métodos Disponibles en Logger

Desde `LogWriter.getLog()`:

**`Debug(string message)`**
- Propósito: Registrar información de depuración detallada
- Nivel: DEBUG
- Uso: Flujos internos, valores de variables, decisiones de código

**`Info(string message)`**
- Propósito: Registrar información general
- Nivel: INFO
- Uso: Inicializaciones, cambios de estado, eventos importantes

**`Warn(string message)`**
- Propósito: Registrar advertencias no críticas
- Nivel: WARN
- Uso: Situaciones inesperadas pero recuperables

**`Error(string message, Exception exception)`**
- Propósito: Registrar errores con detalles de excepción
- Nivel: ERROR
- Uso: Excepciones capturadas, operaciones fallidas

**`Fatal(string message, Exception exception)`**
- Propósito: Registrar errores críticos fatales
- Nivel: FATAL
- Uso: Fallos que impiden continuar

## Configuración

### Cambiar Ubicación de Logs

En `NLog.config`, modificar:
```xml
<target xsi:type="File" 
        name="f" 
        fileName="C:/mi/ruta/custom/sayclip ${shortdate}.log"
        layout="${longdate} ${uppercase:${level}} ${message}" />
```

### Agregar Rotación Diaria

Modificar fileName:
```xml
fileName="${basedir}/logs/sayclip ${date:format=yyyy-MM-dd}.log"
```

### Cambiar Formato de Log

Modificar layout:
```xml
<!-- Incluir nombre de logger y stack trace -->
layout="${longdate} [${level:uppercase=true}] ${logger} - ${message}${exception:format=tostring}"
```

### Agregar Destino Adicional (Consola)

```xml
<target xsi:type="Console" name="c" layout="${longdate} ${uppercase:${level}} ${message}" />

<!-- En rules -->
<logger name="*" minlevel="Debug" writeTo="f,c" />  <!-- Escribe a archivo y consola -->
```

## Integración con Plugins

Plugins pueden usar logging a través de contexto:

```csharp
public bool initialize(ISayclipPluginContext context = null)
{
    context?.Logger?.Info("Plugin inicializado");
    return true;
}

public async Task<string> translate(string text)
{
    _context?.Logger?.Debug($"Traduciendo: {text}");
    
    try
    {
        var result = await TranslateService(text);
        _context?.Logger?.Info($"Traducción exitosa");
        return result;
    }
    catch (Exception ex)
    {
        _context?.Logger?.Error("Error en traducción", ex);
        throw;
    }
}
```

## Ciclo de Vida del Logger

```
1. [Primer Acceso] LogWriter.getLog() llamado por primera vez
   
2. [Inicialización]
   ├─ Se crea instancia estática de Logger
   ├─ Se crea objeto lock para thread-safety
   ├─ Se imprime en consola: "initializing log system"
   └─ Se registra: "logSystem started at {DateTime.Now}"
   
3. [Operación Normal]
   ├─ getLog() retorna instancia existente
   ├─ Código llama métodos del logger
   ├─ Eventos se escriben a archivos de log
   └─ Archivo diario se actualiza
   
4. [Shutdown]
   └─ Recursos se liberan automáticamente
```

## Ventajas del Sistema

- NLog es librería estándar, mantenida activamente
- Configuración flexible sin recompilar código
- Thread-safe para múltiples threads simultáneos
- Rotación automática de logs diarios
- Fácil agregar nuevos destinos (BD, email, etc.)
- Lazy initialization eficiente
- Punto central único (Singleton)

## Casos de Uso Comunes

### Debugging de Plugin

```csharp
_context?.Logger?.Debug($"fromLang={fromLang}, toLang={toLang}");
_context?.Logger?.Debug($"Languages available: {languages.Count()}");
```

### Rastreo de Flujo

```csharp
LogWriter.getLog().Debug("Starting sayclip core");
LogWriter.getLog().Debug("Loading plugins");
LogWriter.getLog().Debug("Setting active plugin");
LogWriter.getLog().Info("Sayclip core ready");
```

### Manejo de Errores

```csharp
catch (Exception ex)
{
    LogWriter.getLog().Error("Error opening clipboard", ex);
    LogWriter.getLog().Warn("Retrying clipboard access");
}
```

## Puntos de Entrada

Proyectos que usan LogWriter:

1. **sayclip** (core) - Logging central de todas las operaciones
2. **sayclipTray** (UI) - Eventos de interfaz gráfica
3. **Plugins** - Opcional, via contexto `ISayclipLogger`
4. **consoleSayclipCoreTester** - Testing del core

## Notas de Rendimiento

- LogWriter usa lock mínimo (solo en init)
- Escritura a disco es asincrónica (NLog lo maneja)
- No bloquea threads de traducción
- Recomendable revisar logs regularmente para no llenar disco

---

**Ubicación**: `sayclip/logSystem/`  
**Dependencia Externa**: NLog (librería)  
**Patrón**: Singleton + Lazy Initialization  
**Thread-Safety**: Sí, mediante lock interno
