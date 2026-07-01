# Documentación del Sistema de Plugins

## Descripción General

El sistema de plugins de sayclip es una arquitectura modular y extensible que permite agregar nuevos servicios de traducción sin modificar el core. Utiliza **MEF (Managed Extensibility Framework)** de .NET para descubrimiento e inyección de dependencias en tiempo de ejecución.

## Arquitectura del Sistema de Plugins

### Flujo de Carga

```
1. [Detección de Paquetes] PluginManager busca *.zip/*.package
   ├─ Extrae en carpeta temporal
   ├─ Respalda plugins existentes si hay colisiones
   └─ Instala contenido en plugins/
   
2. [Descubrimiento MEF] Escanea plugins/ por *.scplug.dll
   
3. [Composición] MEF instancia todas las clases que heredan iSayclipPluginTranslator
   
4. [Inicialización] Se llama initialize() en cada plugin
   
5. [Selección] Se marca el plugin activo según configuración
```

### Estructura de Carpetas

```
Application Root
├─ plugins/
│   ├─ gTranslateBingTranslator/
│   │   ├─ gTranslateBingTranslatorPlugin.scplug.dll
│   │   ├─ GTranslate.dll (dependencia)
│   │   ├─ logSystem.dll
│   │   └─ ...
│   ├─ gTranslateGoogleTranslator/
│   ├─ gTranslateMicrosoftTranslator/
│   └─ ...
├─ plugins.package              ← Paquete instalado (renombrado)
└─ plugins-backup/              ← Backups automáticos de paquetes
    └─ 20260630-143022-plugin-name/
        └─ plugins/
            └─ (copia de seguridad)
```

## Gestión de Paquetes de Plugins

### Detección Automática

`PluginManager.detectPluginPacks()` busca automáticamente:

- Archivos `.zip` con "plugin" en el nombre
- Archivos `.package` con "plugin" en el nombre
- En raíz de la aplicación y en carpeta `plugins/`

### Flujo de Instalación de Paquete

```
1. Validar archivo ZIP válido
2. Extraer en carpeta temporal temporal: unpack-tmp/{guid}/
3. Detectar tipo de paquete:
   a) ¿Contiene carpeta plugins/?
      → Es paquete completo
      → Respaldar plugins/ existente en plugins-backup/
      → Reemplazar plugins/ con contenido del paquete
   
   b) ¿Contiene archivos/carpetas sueltas?
      → Es paquete individual
      → Respaldar solo archivos que colisionan
      → Instalar contenido en plugins/
4. Post-procesamiento:
   - Si deletePluginsPackagesAfterInstall=true: eliminar .zip
   - Si false: renombrar a .installed (marcar como instalado)
5. Limpiar temporal
```

### Backups Automáticos

Los backups se crean en `plugins-backup/{fecha-hora}-{nombre-paquete}/`:

- Antes de instalar paquete completo: respalda plugins/ entero
- Antes de instalar individual: respalda solo archivos con colisión
- Permiten reversión manual si algo falla

### Configuración de Paquetes

**Settings disponibles** en `Properties/Settings`:

- `unpackNewPlugins` (bool, default true) - Habilitar detección automática
- `deletePluginsPackagesAfterInstall` (bool, default false) - Eliminar o renombrar a .installed

## Componentes de PluginManager

### Métodos Principales

**`PluginManager.getInstanse` (Singleton)**
- Punto de entrada al sistema de plugins
- Patrón Lazy<T> para inicialización única
- Constructor privado para forzar singleton

**`void loadPlugins()`**
- Utiliza MEF para descubrir plugins en carpeta `plugins/`
- Crea AggregateCatalog con DirectoryCatalog de:
  - `plugins/*.scplug.dll` (en raíz de plugins)
  - Cada subdirectorio: `plugins/*/\*.scplug.dll`
- Composición: `container.GetExportedValues<iSayclipPluginTranslator>()`

**`void detectPluginPacks()`**
- Busca .zip y .package en raíz y en plugins/
- Para cada candidato: `installPluginPackage()`

**`void installPluginPackage(string packagePath, string appRoot)`**
- Orquesta extracción, respaldo, instalación
- Maneja errores con logging detallado
- Limpia temporales aunque falle

**`void extractPackageSafely(string packagePath, string destinationFolder)`**
- Extrae ZIP validando contra path traversal attacks
- Verifica que todo quede dentro de destinationFolder

**`void backupCompletePluginsFolderIfExists()`**
- Respalda carpeta plugins/ completa si existe y se va a reemplazar

**`void backupCollisionsForIndividualPackage()`**
- Detecta archivos que van a ser sobrescritos
- Respalda solo esos archivos

**`void installIndividualPackageContents()`**
- Copia contenido del paquete individual a plugins/
- Aplica sobrescrituras

**`void postProcessPackageFile()`**
- Renombra a .installed o elimina según configuración

**`void checkActivePluginConfiguration()`**
- Restaura plugin previo guardado en Settings
- Si no existe o lista vacía: selecciona primer disponible
- Si no hay plugins: usa EmptyPlugin (fallback)

**`bool setActivePlugin(string pluginName)`**
- Busca plugin por nombre en lista cargada
- Si existe: marca como activo y guarda en Settings
- Retorna true si tuvo éxito

## Plugins Disponibles

### 1. **gTranslateBingTranslatorPlugin**
- **Servicio**: Microsoft Bing Translator
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~135
- **Configuración**: Ninguna (no tiene ventana config)
- **Línea**: `gTranslateBingTranslatorPlugin.csproj`

### 2. **gTranslateGoogleTranslatorPlugin**
- **Servicio**: Google Translate (API gratuita)
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~244
- **Configuración**: Ninguna

### 3. **gTranslateGoogle2TranslatorPlugin**
- **Servicio**: Google Translator v2
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~244
- **Configuración**: Ninguna

### 4. **gTranslateMicrosoftTranslatorPlugin**
- **Servicio**: Microsoft Translator (GTranslate)
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~136
- **Configuración**: Ninguna (en GTranslate puro)
- **Nota**: Para API key configurable, ver azureTranslatorPlugin

### 5. **gTranslateYandexTranslatorPlugin**
- **Servicio**: Yandex Translator
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~103
- **Configuración**: Ninguna

### 6. **azureTranslatorPlugin** (con configuración)
- **Servicio**: Azure Translator (Microsoft)
- **Requisito**: API key configurable
- **Configuración**: Ventana XAML para ingresar credenciales
- **Tipo**: Plugin más antiguo con UI propia

### 7. **fergunGoogleTranslator**
- **Servicio**: Google Translator (librería Fergun)
- **Librería**: GTranslate 2.2.8
- **Idiomas**: ~105
- **Configuración**: Ninguna

## Ciclo Completo de Plugin

```
[Tiempo de Carga de Aplicación]
1. PluginManager.constructor()
   └─ detectPluginPacks() - instala paquetes pendientes
   └─ loadPlugins() - carga vía MEF
   └─ checkActivePluginConfiguration() - restaura activo

[Tiempo de Ejecución]
2. Sayclip obtiene plugin: PluginManager.getActivePlugin
3. Sayclip inicializa: plugin.initialize(context)
4. Sayclip obtiene idiomas: await plugin.getAvailableLanguages()
5. Usuario selecciona idiomas: plugin.setLanguages()
6. Sayclip traduce: await plugin.translate(text)
7. Si hay UI: plugin.showConfigWindow()

[Cambio de Plugin]
8. Usuario selecciona otro en UI: PluginManager.setActivePlugin()
9. Se repite ciclo 3-7 con nuevo plugin

[Shutdown]
10. Recursos se liberan automáticamente
```

## MEF (Managed Extensibility Framework)

### Cómo Funciona la Composición

1. **[Export]** - Plugin declara su implementación:
   ```csharp
   [Export(typeof(iSayclipPluginTranslator))]
   public class Translator : iSayclipPluginTranslator { }
   ```

2. **[ImportMany]** - PluginManager declara qué importar:
   ```csharp
   [ImportMany]
   private IEnumerable<Lazy<iSayclipPluginTranslator>> plugins;
   ```

3. **Descubrimiento** - DirectoryCatalog escanea *.scplug.dll:
   ```csharp
   var catalog = new DirectoryCatalog("plugins", "*.scplug.dll");
   ```

4. **Composición** - Se inyectan automáticamente:
   ```csharp
   container = new CompositionContainer(catalog);
   container.ComposeParts(this);  // Aquí se llena 'plugins'
   ```

### Ventajas de MEF

- ✅ Descubrimiento automático de plugins
- ✅ Sin referencias explícitas al dll del plugin
- ✅ Plugins pueden existir en carpeta plugins/ sin estar en sln
- ✅ Carga en tiempo de ejecución
- ✅ Fácil testing (puede reemplazarse implementación mock)

## Testing de Plugins

### Herramientas de Testing

**PluginRuntimeProbe** (`tests/tools/PluginRuntimeProbe/`)
- Carga todos los plugins desde carpeta plugins/
- Prueba cada uno aisladamente
- Valida: inicialización, idiomas, traducción básica
- Modo estricto, sin traducción, ruta customizable

Uso:
```bash
dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug
```

**PluginPackageCoreProbe** (`tests/tools/PluginPackageCoreProbe/`)
- Importa el core completo
- Crea paquete de plugin
- Prueba instalación automática
- Valida carga vía PluginManager

## Requisitos para Plugin Nuevo

- Heredar de `iSayclipPluginTranslator`
- Agregar atributo `[Export(typeof(sayclip.iSayclipPluginTranslator))]`
- Implementar todos los métodos de interfaz
- Nombrar DLL como `{NombrePlugin}.scplug.dll`
- Referencia a `sayclip.contracts.dll` (no al core completo)
- Validación en PluginRuntimeProbe

## Puntos de Extensión

1. **Nuevas fuentes de traducción**: Crear nuevo plugin
2. **Formatos de paquete**: Extender `detectPluginPacks()`
3. **Servicios de contexto**: Agregar a `ISayclipPluginContext`
4. **Auto-configuración**: Extender `initialize()`

---

Para aprender a crear tu propio plugin, consulta: [Plugin Creation Guide](./plugin-creation-guide.md)

**Arquitectura**: MEF (Managed Extensibility Framework)  
**Ciclo de Vida**: Lazy initialization con Singleton pattern  
**Descubrimiento**: Convención sobre configuración (carpeta + atributo Export)
