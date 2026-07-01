# Instrucciones para Copilot en tfj-sayclip

Este repositorio es una aplicacion de accesibilidad y traduccion en C#/.NET con arquitectura modular por plugins.

## Tipo de proyecto

- Aplicacion de escritorio para Windows (WPF) con enfoque en accesibilidad.
- Core de traduccion y portapapeles desacoplado de la UI.
- Sistema de plugins para proveedores de traduccion, cargados dinamicamente en runtime.
- Solucion principal: `sayclip/sayclip.sln`.

## Lo mas importante del proyecto

- La UI principal esta en `sayclip/sayclipTray`.
- El core esta en `sayclip/sayclip`.
- Los contratos de plugins estan en `sayclip/sayclip.contracts`.
- Logging centralizado en `sayclip/logSystem`.
- Los plugins compilan con salida a `sayclip/sayclip/plugins/<PluginFolder>/` y usan nombre de ensamblado `*.scplug.dll`.

## Flujo de build/release

- El flujo de GitHub es simple: al subir un tag se compila y se publica release.
- No agregar complejidad innecesaria en CI si no es requerida.

## Estructura de proyecto (resumen operativo)

- `sayclip/sayclip`:
  - `Sayclip.cs`: flujo principal de monitoreo/traduccion.
  - `PluginManager.cs`: descubrimiento/carga de plugins por MEF y seleccion de plugin activo.
  - `ConfigurationManager.cs`: acceso a configuracion persistida.
- `sayclip/sayclipTray`:
  - UI WPF, tabs de configuracion general/plugins, icono de bandeja.
- `sayclip/sayclip.contracts`:
  - `iSayclipPluginTranslator` y contratos de contexto/accesibilidad/log.
- `sayclip/logSystem`:
  - NLog y fachada de logger (`LogWriter`).
- `docs/`:
  - Documentacion funcional y tecnica del proyecto.

## Comandos recomendados

### Build local en Linux (cross-targeting Windows)

Usar `EnableWindowsTargeting=true` para proyectos Windows:

```bash
cd sayclip
dotnet build ./sayclipTray/sayclipTray.csproj -c Release -p:Platform=x86 -p:EnableWindowsTargeting=true
```

Para plugin individual (ejemplo):

```bash
cd sayclip
dotnet build ./openRouterDotNetTranslatorPlugin/openRouterDotNetTranslatorPlugin.csproj -c Debug -p:Platform=x86 -p:EnableWindowsTargeting=true
```

### Build/publish en Windows

```bash
cd sayclip
dotnet publish ./sayclipTray/sayclipTray.csproj -c Release -p:Platform=x86
```

En este repositorio, `Release` en la UI usa publicacion self-contained para `win-x86`.

## Limitaciones de plataforma

- Compilacion y ejecucion orientadas a x86 por dependencias de accesibilidad/lectores de pantalla.
- Evitar AnyCPU/x64 para la app final si rompe compatibilidad con dependencias nativas.
- Al ejecutar comandos desde Linux contra proyectos Windows, habilitar `EnableWindowsTargeting`.

## Estructura y convenciones de plugins

### Requisitos base

- Implementar `iSayclipPluginTranslator` (proyecto contracts).
- Exportar con MEF: `[Export(typeof(sayclip.iSayclipPluginTranslator))]`.
- Nombre de ensamblado con sufijo `.scplug`.
- Salida de build del plugin a carpeta bajo `sayclip/sayclip/plugins/`.

### Configuracion de proyecto plugin

- `TargetFramework`: `net9.0-windows`.
- `Platforms`: `AnyCPU;x86`.
- `PlatformTarget` condicional para x86.
- Configuraciones declaradas: `Debug;Release;release-small`.
- Referencia a `sayclip.contracts`.

### Configuracion del plugin (UI)

- Si el plugin requiere configuracion, usar ventana WPF propia.
- Persistir settings en `Properties/Settings.settings`.
- Idiomas de interfaz via `ResourceDictionary` (es/en) cuando aplique.
- Si falta una configuracion critica (ej. apiKey), devolver mensaje de cortesia y registrar warning, en vez de romper el flujo del core.

## Como hemos documentado

- Documentacion central en carpeta `docs/`.
- Separacion por tema (index, core, ui, plugins, contracts, logging, guia de plugins).
- Lenguaje directo y practico, enfocado a mantenimiento y extension.

## Estilo de programacion y redaccion en este repo

- Comentarios simples y utiles.
- Evitar bloques de resumen al inicio de archivos.
- Sin emojis.
- Mantener nomenclatura existente del proyecto:
  - camelCase para metodos/variables donde ya se usa asi.
  - PascalCase/CamelUpperCase para tipos/miembros segun convencion de C# y segun archivo existente.
- No reescribir mas de lo necesario cuando se hacen cambios.
- Priorizar cambios acotados y compatibles con el estilo actual del archivo.

## Criterios al modificar codigo

- No cambiar arquitectura sin necesidad.
- No romper compatibilidad del sistema de plugins.
- Preferir implementaciones simples y mantenibles.
- Si hay que agregar nuevos plugins, mantener el mismo patron de csproj, salida, settings y configuracion de solucion.
