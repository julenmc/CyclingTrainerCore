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
* Add async method with multiple tasks for the interval detection. One task for each interval time span (short, medium or long).
* Add sprints to final return of intervals service.
* Eliminar el repositorio IntervalRepository.
* Igual no siempre hay que recortar el intervalo más corto. Mirar el test "CollisionInsideAtStart"; aquí tendría más sentido cortar el largo y mantener el corto.

## Known issues
* No se están buscando los sprints.
* En el test de la consola el intervalo de las 15:54:00 y el de las 15:59:42 no tienen sentido, empiezan más tarde de lo que deberían. Los intervalos cortos anteriores y posteriores no tienen el mismo problema.