# Documentación de la Interfaz Gráfica (UI)

## Descripción General

La interfaz gráfica de sayclip es minimalista y se integra en la bandeja del sistema (system tray). Proporciona:
- Ícono en bandeja del sistema para control rápido
- Ventana de configuración con pestañas (General, Plugins)
- Gestión de opciones de traducción
- Selección de idiomas y plugins
- Acceso a la configuración de plugins individuales

## Componentes Principales

### 1. **App.xaml / App.xaml.cs** - Punto de Entrada

Inicializa la aplicación WPF y lanza el core en un thread separado.

#### Métodos Clave a Estudiar

- **`App_Startup(object sender, StartupEventArgs e)`** - Se ejecuta al iniciar:
  - Carga diccionario de idiomas desde archivos de recursos
  - Instancia MainWindow
  - Lanza `Sayclip.Main()` en thread de background
  - Configura handlers de error no capturados

- **`App_DispatcherUnhandledException()`** - Manejo de excepciones globales

#### Recursos Importantes

- Diccionario de idiomas: `/lang/` (archivos XAML con ResourceDictionary)
- Variables globales: `Sayclip.dictlang` - Acceso al diccionario desde código

### 2. **MainWindow.xaml / MainWindow.xaml.cs** - Ventana Principal

Contenedor de la interfaz con dos pestañas principales.

#### Estructura XAML

```xaml
MainWindow
  ├─ TabControl
  │   ├─ GeneralTab (configuración general)
  │   └─ PluginsTab (gestión de plugins)
  └─ Botones (Aplicar, Descartar, Salir)
```

#### Métodos Clave a Estudiar

- **`MainWindow()` (constructor)** - Inicializa componentes, configura event handlers
- **`MainWindow_KeyDown()`** - Escucha Esc para cerrar ventana
- **`MainWindow_Closing()`** - Intercepta cierre y lo convierte en Hide()
- **`exitButton_Click()`** - Cierra la aplicación completamente
- **`applyButton_Click()`** - Guarda cambios y cierra ventana
- **`discardButton_Click()`** - Descarta cambios y cierra ventana
- **`MainWindow_GotFocus()`** - Actualiza controles cuando ventana recibe foco

#### Comportamiento

- La ventana se oculta en bandeja, no se cierra, permitiendo relanzar desde ícono
- Presionar Esc también oculta la ventana
- Botones Apply/Discard guardan/descartan configuración

### 3. **NotifyIconResources.xaml / NotifyIconViewModel.xaml.cs** - Bandeja del Sistema

Gestiona el ícono en la bandeja del sistema y sus acciones.

#### Métodos Clave a Estudiar

- Menú contextual del ícono (mostrar, ocultar, salir)
- Control de visibilidad de MainWindow
- Eventos de click en ícono

### 4. **GeneralTab.xaml / GeneralTab.xaml.cs** - Configuración General

Pestaña para configuración general de la aplicación.

#### Controles Principales

- **ComboBox de Traductor**: Selecciona plugin de traducción
- **ComboBoxes de Idiomas**: Selecciona idioma origen y destino
- **Checkbox de Estado**: On/Off del monitoreo
- **Hotkey Setup**: Configurar atajos de teclado

#### Métodos Clave a Estudiar

- **`ComboBoxTranslators_SelectionChanged()`** - Al cambiar plugin:
  - Obtiene idiomas disponibles del nuevo plugin
  - Recarga ComboBoxes de idiomas
  - Inicializa el plugin con `initialize()`

- **`ComboBoxFromLang_SelectionChanged()`** - Al cambiar idioma origen:
  - Actualiza configuración del plugin: `plugin.setLanguages()`
  - Guarda en Settings

- **`ComboBoxToLang_SelectionChanged()`** - Al cambiar idioma destino:
  - Actualiza configuración del plugin: `plugin.setLanguages()`
  - Guarda en Settings

- **`updateControlsConfigs()`** - Sincroniza UI con valores guardados (llamado al recibir foco)

#### Flujo de Selección de Idiomas

```
Usuario selecciona plugin en ComboBoxTranslators
  ↓
ComboBoxTranslators_SelectionChanged():
  - Obtiene plugin: PluginManager.getPluginsNames()
  - Obtiene idiomas: plugin.getAvailableLanguages()
  - Carga ComboBoxes (origen/destino)
  - Si plugin necesita inicialización: plugin.initialize()
  ↓
Usuario selecciona idioma origen
  ↓
ComboBoxFromLang_SelectionChanged():
  - plugin.setLanguages(origen, destino)
  - ConfigurationManager.fromLanguage = origen
  ↓
Ídem para destino
```

### 5. **PluginsTab.xaml / PluginsTab.xaml.cs** - Gestión de Plugins

Pestaña para gestionar plugins de traducción disponibles.

#### Controles Principales

- **ListBox de Plugins**: Lista todos los plugins cargados
- **Botón Config**: Abre ventana de configuración del plugin seleccionado

#### Métodos Clave a Estudiar

- **`PluginsTab()` (constructor)** - Inicializa lista de plugins
- **`PluginsListBox_SelectionChanged()`** - Al seleccionar plugin en lista
- **`configButton_Click()`** - Abre ventana de configuración:
  - Verifica si plugin tiene config: `plugin.haveConfigWindow()`
  - Si es true: `plugin.showConfigWindow(idioma)`

#### Flujo de Configuración de Plugin

```
Usuario selecciona plugin en lista
  ↓
PluginsListBox_SelectionChanged():
  - Obtiene plugin desde lista cargada
  - Valida si haveConfigWindow()
  - Habilita/deshabilita botón Config
  ↓
Usuario hace click en botón Config
  ↓
configButton_Click():
  - plugin.showConfigWindow(idioma_UI)
  - Se abre ventana WPF del plugin (si existe)
```

## Archivos de Recursos

### Diccionarios de Idiomas (`/lang/`)

Archivos XAML con ResourceDictionary para textos multiidioma:

```
/lang/
  ├─ en.xaml (inglés)
  ├─ es.xaml (español)
  └─ ...
```

Acceso desde código:
```csharp
string texto = Sayclip.dictlang["clave"].ToString();
```

### Estilos y Recursos Visuales

- `NotifyIconResources.xaml` - Estilos del ícono en bandeja
- Colores y fuentes definidas en App.xaml

## DelegateCommand.cs - Patrón MVVM

Implementación simple de `ICommand` para binding de botones en XAML.

#### Uso Típico

```csharp
var command = new DelegateCommand(
  execute: () => { /* acción */ },
  canExecute: () => { /* condición */ }
);
```

## Flujo Completo de Inicialización UI

```
Program.Main()
  ↓
new App() → App_Startup()
  ↓
Carga diccionario de idiomas: Sayclip.dictlang
  ↓
Crea MainWindow
  ↓
Muestra MainWindow
  ↓
Lanza Thread: Sayclip.Main(cancellationToken)
  ↓
MainWindow esperando interacción del usuario
```

## Ciclo de Vida de la Ventana

```
1. Abierta por usuario
   ↓
2. Modificación de configuración
   ↓
3. Click Apply → guarda configuración
   ↓
4. Ventana se oculta (Hide)
   ↓
5. Ícono en bandeja permite reabrirla
   ↓
6. Ciclo repite
```

## Temas Clave para Estudiar

1. **Data Binding**: Cómo se vinculan controles a propiedades
2. **XAML Layout**: Estructura de Grid, StackPanel, etc.
3. **Event Handling**: Eventos de SelectionChanged, Click, etc.
4. **Resource Access**: Diccionarios de idiomas
5. **Threading**: Cómo el UI se mantiene responsivo mientras el core trabaja

## Puntos de Extensión

1. Agregar nuevas pestañas en TabControl (GeneralTab, PluginsTab)
2. Añadir controles en GeneralTab para nuevas opciones
3. Crear nuevos archivos de idioma en `/lang/`
4. Extender NotifyIcon con más opciones de menú

---

**Arquitectura**: WPF con patrón simple (sin MVVM formal)  
**Threading**: UI en thread principal, Core en thread separado  
**State**: Configuración persistida en `sayclipTray.Properties.Settings`
