# TrainingDatabase
## Introducción
El módulo SessionReader se encarga de la lectura y escritura de la base de datos que contiene la información necesaria para el funcionamiento de una aplicación que utilice el framework [CyclingTrainer](../README.md). Contiene ciclistas, sesiones de enrtenamiento, intervalos de dichas sesiones, puertos de montaña... 

> [!WARNING]
> Este módulo está pendiente de refactorizar y mejorar. No está listo para usarse.

El módulo se divide en cuatro diferentes proyectos:
* **Core**: La biblioteca principal del módulo con la lógica de negocio.
* **Test**: Proyecto de test unitarios utilizando MSTest. 
* **Console**: Una aplicación de consola para interactuar con el módulo. Sirve para realizar test de integración e interactuar directamente con la base de datos.
* **WebApi**: API web para la interacción la base de datos. Pendiente de desarrollar.

## Principales funcionalidades
Se podrán guardar y leer los siguientes datos:
    * Ciclistas: nombre y apellidos, fecha de nacimiento, género...
    * Evolución de ciclista: altura, peso, vo2max, curva de potencia.
    * Sesión: fecha, distancia, calorías, potencia media... y la ruta donde se encuentra el archivo original.
    * Intervalos: fecha, tiempo, distancia, potencia media...
    * Subidas: Nombre, posición del inicio, posición del final, distancia... y la ruta donde se encuentra el archivo .gpx de solo la subida.

## Funcionalidades a futuro ("To Do")
* Refactorizar código utilizando Entity Framework.
* Crear la API web para la interacción con la base de datos a través de HTTP.