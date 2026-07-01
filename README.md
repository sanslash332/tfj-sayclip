# tfj-sayclip
Una herramienta que lee en voz alta con el lector de pantalla activo, el texto copiado al portapapeles. Ofrece traducción instantánea. De manera que reproduce la traducción del texto copiado, en vez del texto en si

# Homepage
[https://www.tiflojuegos.com/news](https://www.tiflojuegos.com/news)

## Descarga:

Para descargar la última versión, bájala directamente desde los [releases](https://github.com/sanslash332/tfj-sayclip/releases/latest)
El programa incluye actualizador automático.

## Detalle de compilación:

Debido a dependencias, para que el programa funcione tiene que ser compilado obligatoriamente en plataforma x86. No funciona en anyCPU o x64.

## resumen simple de arquitectura:

El programa se divide en:

* Un núcleo(sayclip core): que opera toda la lógica de manejo del portapapeles, traducción, configuraciones y manejo de plugins.
* Un plugin por cada servicio de traducción disponible que el core carga en runtime.
* Un proyecto de interfaces (sayclip.contracts): donde se definen las bases de las firmas que usan los plugins. Proyecto usado para poder crear nuevos plugins sin tener que pasar por el core.
* ULa interfás gráfica (sayclip trai): que incorpora un ícono en la barra de sistema, y una interfás gráfica minimalista para configurar el programa.
* Un proyecto para gestionar el logging
* y otros proyectos para ejecutar pruebas del core, o los plugins en consola.


Más detalles en específico puedes leerlo en [la documentación](./docs/index.md)

## Licencia

This project is under the [mit licence](LICENSE)

## References

Fergun google translator plugin include code from the awesome [Fergun bot project](https://github.com/d4n3436/Fergun/)
And the google translator plugin is based on [google translate free api](https://github.com/wadereye/GoogleTranslateFreeApi)

## Other projects and dependencies used

In addition to Fergun projects, this project also uses work/libraries from:

- [GTranslate](https://github.com/d4n3436/GTranslate)
- [UniversalSpeech](https://github.com/qtnc/UniversalSpeech)
- [NLog](https://github.com/NLog/NLog)
- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET)
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon)
- [SharpClipboard](https://github.com/Willy-Kimura/SharpClipboard)

