# Documentación del Core (Núcleo)

## Descripción General

El core de sayclip es el motor central que orquesta todo el funcionamiento de la aplicación. Gestiona el monitoreo del portapapeles, la interacción con plugins de traducción, la reproducción de voz, caché de resultados y las configuraciones globales.

## Componentes Principales

### 1. **Sayclip.cs** - Orquestador Principal

Clase principal que implementa la lógica de flujo. Es responsable de:
- Inicialización del sistema y obtención del plugin activo
- Monitoreo del portapapeles mediante `SharpClipboard`
- Orquestación del flujo de traducción
- Reproducción de audio vía lector de pantalla
- Gestión de caché para evitar traducciones duplicadas

#### Métodos Clave a Estudiar

- **`Main(CancellationToken canceller)`** - Inicializa el core, configura listeners del portapapeles
- **`SharpCP_ClipboardChanged(object sender, SharpClipboardEventArgs e)`** - Ejecutado al detectar cambio en portapapeles
- **`translateAndSay(string txt)`** - Flujo principal: traduce texto y lo reproduce
- **`sayAndCopy(string txt)`** - Reproduce audio y copia al portapapeles
- **`repeatLastClipboardContent()`** - Repite la última traducción
- **`shutDownCore()`** - Limpia recursos y cierra monitores

#### Flujo de Traducción Completo

```
1. Portapapeles cambia → SharpCP_ClipboardChanged() activado
2. Validar si es texto y si no es duplicado (caché)
3. Obtener plugin activo desde PluginManager
4. Llamar plugin.translate(texto)
5. Si es válido, reproducir audio vía ScreenReaderControl.speech()
6. Copiar resultado al portapapeles
7. Guardar en caché para evitar duplicados
```

### 2. **PluginManager.cs** - Gestor de Plugins

Gestor centralizado de plugins usando MEF (Managed Extensibility Framework). Es responsable de:
- Descubrimiento automático de plugins desde carpeta `plugins/`
- Instanciación y composición de plugins
- Carga dinámica de ensamblados en tiempo de ejecución
- Detección e instalación de paquetes de plugins
- Gestión del plugin activo

#### Métodos Clave a Estudiar

- **`PluginManager` (constructor)** - Inicializa MEF, detecta paquetes, carga plugins
- **`reloadPlugins()`** - Recarga lista de plugins (útil para testing)
- **`setActivePlugin(string pluginName)`** - Establece el traductor activo
- **`getPluginsNames()`** - Devuelve lista de nombres de plugins disponibles
- **`detectPluginPacks()`** - Detecta `.zip` / `.package` de plugins en la raíz
- **`loadPlugins()`** - Carga plugins mediante MEF desde carpeta `plugins/`
- **`checkActivePluginConfiguration()`** - Restaura plugin guardado en configuración o establece primero disponible

#### Flujo de Carga de Plugins

```
1. MEF busca ensamblados en carpeta plugins/ con patrón *.scplug.dll
2. Para cada subdirectorio en plugins/, busca el mismo patrón
3. Se componen dinámicamente todas las clases que heredan iSayclipPluginTranslator
4. Si existen paquetes (.zip/.package), se extraen e instalan antes
5. Se selecciona plugin activo según configuración o primer disponible
```

### 3. **Translation.cs** - Lógica de Traducción

Encapsula la lógica relacionada con traducción y gestión de resultados.

#### Métodos Clave a Estudiar

- Métodos para validar texto traducido
- Almacenamiento de resultados en caché
- Formateo de salida

### 4. **ConfigurationManager.cs** - Gestor de Configuración

Singleton que centraliza acceso a configuración persistente almacenada en `Properties/Settings`.

#### Métodos/Propiedades Clave a Estudiar

- **`getInstance`** - Obtiene instancia singleton
- **`translator`** - Get/Set del nombre del plugin activo
- **`unpackNewPlugins`** - Flag para detectar paquetes de plugins
- **`deletePluginsPackagesAfterInstall`** - Flag para eliminar o renombrar paquetes después de instalar

### 5. **ScreenReaderControl.cs** - Control de Accesibilidad

Interactúa con lectores de pantalla (NVDA, JAWS, Narrador) para reproducir audio.

#### Métodos Clave a Estudiar

- **`speech(string text, bool interrupt)`** - Reproduce texto en voz alta
- Soporte para múltiples APIs de lector de pantalla

## Flujos Importantes

### Flujo 1: Inicio de la Aplicación

```
sayclipTray.Program.Main()
  ↓
App.xaml.cs construye MainWindow
  ↓
MainWindow.xaml.cs carga tabs (General, Plugins)
  ↓
Se llama Sayclip.Main(cancellationToken) en thread separado
  ↓
Sayclip.Main():
  - Obtiene PluginManager.getInstance
  - Obtiene plugin activo: PluginManager.getActivePlugin
  - Crea SharpClipboard para monitorear portapapeles
  - Se queda esperando cambios en ClipboardChanged
```

### Flujo 2: Detección de Plugin Activo

```
PluginManager constructor:
  ↓
checkActivePluginConfiguration():
  - Lee propiedades.settings.translator
  - Busca plugin con ese nombre en lista cargada
  - Si existe, setActivePlugin(nombre)
  - Si no existe o lista vacía, setActivePlugin(primer disponible)
  - Si no hay plugins, usa EmptyPlugin (fallback)
```

### Flujo 3: Traducción en Tiempo Real

```
Usuario copia texto al portapapeles
  ↓
SharpClipboard detecta cambio → SharpCP_ClipboardChanged()
  ↓
Validar:
  - ¿Es texto valido y no nulo?
  - ¿Es diferente del último resultado traducido? (caché)
  - ¿Es suficientemente diferente del último copiado? (debounce)
  ↓
Obtener plugin activo
  ↓
plugin.translate(texto) → Task<string>
  ↓
Reproducir resultado:
  - ScreenReaderControl.speech(resultado)
  ↓
Copiar resultado al portapapeles:
  - setClipboardText(resultado)
  ↓
Guardar en caché
```

## Configuración Persistente

Las configuraciones se almacenan en `Properties/Settings.settings` y `Settings.Designer.cs`:

```
- translator: nombre del plugin traducción activo
- fromLanguage: idioma origen seleccionado
- toLanguage: idioma destino seleccionado
- globalCache: activar/desactivar caché global
- unpackNewPlugins: detectar paquetes .zip/.package
- deletePluginsPackagesAfterInstall: limpiar o renombrar después instalar
```

## Dependencias Externas

- **SharpClipboard** - Monitoreo de portapapeles en tiempo real
- **NLog** - Logging de eventos y errores
- **System.ComponentModel.Composition (MEF)** - Carga dinámica de plugins
- **UniversalSpeech** - Interfaz unificada para lectores de pantalla

## Puntos de Extensión

- **PluginManager**: Modificar `detectPluginPacks()` para soportar otros formatos
- **Sayclip**: Agregar eventos de pre/post traducción
- **ScreenReaderControl**: Añadir soporte para nuevos lectores de pantalla
- **ConfigurationManager**: Extender con nuevas opciones globales

## Testing

Para testing del core sin UI, usa:
- `consoleSayclipCoreTester` - Proyecto de consola para probar core
- `PluginRuntimeProbe` - Herramienta para validar carga de plugins

---

**Métodos de entrada principales**:
- `Sayclip.Main()` - Punto de entrada del core
- `PluginManager.getInstanse` - Acceso al gestor de plugins
- `ConfigurationManager.getInstance` - Acceso a configuración
