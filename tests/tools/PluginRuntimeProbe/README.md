# PluginRuntimeProbe

Herramienta de verificación runtime para plugins `.scplug.dll` de sayclip.

## Qué valida

- Descubrimiento de ensamblados de plugin.
- Carga MEF por plugin (aislada, uno a uno).
- Activación con `initialize(null)`.
- Consulta de idiomas con `getAvailableLanguages("en")`.
- Smoke test de traducción con texto `hello world`.

## Comandos

Desde la raíz del repositorio:

- Ejecución normal (omite en Linux los fallos de dependencia WPF como skip de plataforma):

  `dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug`

- Modo estricto (cualquier fail o skip devuelve código de salida 1):

  `dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug -- --strict`

- Sin smoke test de traducción:

  `dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug -- --no-translate-smoke`

- Ruta explícita de plugins:

  `dotnet run --project tests/tools/PluginRuntimeProbe/PluginRuntimeProbe.csproj --configuration Debug -- --plugins-root /ruta/a/plugins`

## Códigos de salida

- `0`: todo OK (o solo skips de plataforma en modo no estricto).
- `1`: hay fallos (y en modo estricto también si hubo skips de plataforma).
- `2`: no se encontró carpeta de plugins.
- `3`: no se encontraron archivos `.scplug.dll`.
