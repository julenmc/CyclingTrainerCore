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
* Intervalos:
    * Añadir método asíncrono con llamadas en diferentes tareas para cada periodo definido (sprint, corto, medio y largo).
    * Igual no siempre hay que recortar el intervalo más corto. Mirar a futuro.
    * Probar si durante el refinado se pueden perder sprints.
    * La colisión de los intervalos se gesitionará antes de asignarlos como sub-intervalos.

## Known issues
* En el test de la consola el intervalo de las 15:54:00 y el de las 15:59:42 no tienen sentido, empiezan más tarde de lo que deberían. Los intervalos cortos anteriores y posteriores no tienen el mismo problema.
* En un merge hay veces que se puede quedar el sub-intervalo con menor potencia que el principal.