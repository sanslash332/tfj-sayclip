# Documentación de tfj-sayclip

## Resumen del Proyecto

**tfj-sayclip** es una herramienta de accesibilidad que lee en voz alta con el lector de pantalla activo el texto copiado al portapapeles, ofreciendo traducción instantánea. La principal característica es que reproduce la traducción del texto en lugar del texto original, permitiendo acceso rápido a contenido en otros idiomas.

### Características Principales

- Monitoreo automático del portapapeles
- Traducción en tiempo real mediante plugins modulares
- Integración con lectores de pantalla (NVDA, JAWS, etc.)
- Interfaz gráfica minimalista con configuración en bandeja del sistema
- Sistema de plugins extensible para múltiples servicios de traducción
- Actualizador automático

## Arquitectura General

El proyecto está diseñado en capas modular:

```
┌─────────────────────────────────────────────┐
│          UI (sayclipTray)                   │
│   - MainWindow (interfaz gráfica)           │
│   - Configuración de opciones               │
├─────────────────────────────────────────────┤
│          CORE (sayclip)                     │
│   - PluginManager (carga/gestión)           │
│   - Sayclip (orquestación principal)        │
│   - Translation (flujo de traducción)       │
│   - Acceso al portapapeles                  │
├─────────────────────────────────────────────┤
│      Interfaces/Contratos                   │
│   - sayclip.contracts (interfaces plugin)   │
├─────────────────────────────────────────────┤
│   Plugins (servicios de traducción)         │
│   - GTranslateBingTranslatorPlugin          │
│   - GTranslateGoogleTranslatorPlugin        │
│   - GTranslateMicrosoftTranslatorPlugin     │
│   - GTranslateYandexTranslatorPlugin        │
│   - y otros...                              │
├─────────────────────────────────────────────┤
│        Soporte                              │
│   - logSystem (logging)                     │
│   - AssemblySettings (configuración)        │
└─────────────────────────────────────────────┘
```

### Componentes Clave

1. **[Core (Núcleo)](./core.md)** - Lógica central que gestiona portapapeles, traducción y orquestación
2. **[UI (Interfaz Gráfica)](./ui.md)** - Ventana de configuración y control en bandeja del sistema
3. **[Sistema de Plugins](./plugins.md)** - Arquitectura modular para servicios de traducción
4. **[Contratos/Interfaces](./contracts.md)** - Definición de interfaces que implementan los plugins
5. **[Sistema de Logging](./logging.md)** - Registro centralizado de eventos y errores

## Requisitos de Compilación

⚠️ **IMPORTANTE**: Debido a dependencias de accesibilidad (UniversalSpeech), el programa **solo compila en x86**. No funciona en AnyCPU o x64.

Compilar siempre con:
```bash
dotnet build --configuration Debug -p:Platform=x86
# o
msbuild -p:Configuration=Debug -p:Platform=x86
```

## Flujo Principal

1. **Inicio**: `sayclipTray.Program.Main()` inicia la aplicación UI y notifica al core
2. **Core activa**: `Sayclip.Main()` inicia el monitoreo del portapapeles
3. **Detección**: `SharpCP_ClipboardChanged` se ejecuta cuando hay cambios
4. **Traducción**: El texto se pasa al plugin activo vía `iSayclipPluginTranslator.translate()`
5. **Reproducción**: El resultado se lee en voz alta y se copia al portapapeles
6. **Caché**: Último resultado se almacena para evitar duplicados

## Estructura de Carpetas

```
/sayclip
  /sayclip              → Núcleo principal (DLL)
  /sayclipTray          → Interfaz gráfica (UI)
  /sayclip.contracts    → Interfaces para plugins
  /logSystem            → Sistema de logging
  /gTranslate*          → Plugins de traducción
  /tests/tools          → Herramientas de testing
```

## Documentación Detallada

Consulta los siguientes documentos para mayor profundidad:

- **[Core Documentation](./core.md)** - Métodos principales, flujos y cómo opera el portapapeles
- **[UI Documentation](./ui.md)** - Estructura XAML, eventos y configuración
- **[Plugin System](./plugins.md)** - Cómo funcionan los plugins y el sistema MEF
- **[Plugin Creation Guide](./plugin-creation-guide.md)** - Paso a paso para crear un plugin
- **[Contracts Documentation](./contracts.md)** - Interfaces que todo plugin debe implementar
- **[Logging Documentation](./logging.md)** - Sistema centralizado de registro de eventos

## Notas de Desarrollo

- El proyecto usa **MEF (Managed Extensibility Framework)** para carga dinámica de plugins
- Se utiliza **SharpClipboard** para monitoreo en tiempo real del portapapeles
- **NLog** maneja logging y debugging
- Los plugins se cargan desde la carpeta `plugins/` relativa a la aplicación
- La solución soporta múltiples configuraciones: Debug, Release, release-small

---

**Última actualización**: Junio 2026  
Para más información, consulta el [README principal](../README.md) o los releases en [GitHub](https://github.com/sanslash332/tfj-sayclip/releases)
