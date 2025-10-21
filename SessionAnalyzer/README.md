# SessionAnalyzer
## Introducción
El módulo SessionAnalyzer se encarga del análisis de una sesión específica a través de los datos biométricos leídos en con SessionReader.

El módulo se divide en tres diferentes proyectos:
* **Core**: La biblioteca principal del módulo con la lógica de negocio.
* **Test**: Proyecto de test unitarios utilizando MSTest. 
* **Console**: Una aplicación de consola para interactuar con el módulo. Sirve para realizar test de integración con archivos .fit reales.

## Principales funcionalidades
Estas son las principales funcionalidades de la biblioteca:
* Cálculo de **potencia media** y **curva de potencia**. Pendiente de refactorizar utilizando las herramientas creadas para la búsqueda de los intervalos.
* Búsqueda y detección de **intervalos de potencia**, para más información ir a las [especificaciones](docs/IntervalsSpecifications.md).

## Funcionalidades a futuro ("To Do")
* Cálculo de datos típicos como la potencia normalizada, factor de intensidad, carga...
* Desacople aeróbico.