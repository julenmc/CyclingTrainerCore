# SessionReader
## Introducción
El módulo SessionReader se encarga de la lectura de sesiones de entrenamiento en formato .fit o .gpx.

El módulo se divide en tres diferentes proyectos:
* **Core**: La biblioteca principal del módulo con la lógica de negocio.
* **Test**: Proyecto de test unitarios utilizando MSTest. 
* **Console**: Una aplicación de consola para interactuar con el módulo. Sirve para realizar test de integración con archivos .fit o .gpx reales.

## Principales funcionalidades
Estas son las principales funcionalidades de la biblioteca:
* **Lectura de archivos** .fit o .gpx y guardado de los datos (posición, potencia, velocidad, FC...) de forma dinámica utilizandos los modelos de la biblioteca.
* **Procesamiento de la ruta** para su suavizado.
* **Búsqueda de subidas** en la ruta de la sesión según la configuración.

## Funcionalidades a futuro ("To Do")
* Detección de subidas configurable.