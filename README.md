# Librería CyclingTrainer

Framework que incluye diferentes herramientas (módulos) para un "entrenador virtual" de ciclismo. 

> [!CAUTION]
> Estas módulos están en proceso de desarrollo, por lo que no están listos y se pueden encontrar algunos errores.

Los módulos activos son:
* [CyclingTrainer.Core](CyclingTrainer.Core/README.md): esta biblioteca es la base del framework. Contiene modelos y constantes que no pertenecen a ninguna biblioteca en particular.
* [TrainingDatabase](TrainingDatabase/README.md): módulo de lectura y escritura de la base de datos que contiene la información necesaria para el funcionamiento de una aplicación que utilice el framework. Contiene ciclistas, sesiones de enrtenamiento, intervalos de dichas sesiones, puertos de montaña... Este módulo está pendiente de refactorizar y mejorar. No está listo para usarse.
* [SessionReader](SessionReader/README.md): módulo que se encarga de la lectura de sesiones de entrenamiento en formato .fit o .gpx.
* [SessionAnalyzer](SessionAnalyzer/README.md): módulo que analiza los datos biométricos leídos a través del proyecto [SessionReader](SessionReader/README.md).

También se encuentran módulos obsoletos o pendientes de incorporar a la solución:
* DeviceInterface: pensado para definir las interfaces de dispositivos electrónicos (potenciómetros, sensores de FC o incluso rodillos inteligentes).
* Cyclist: contiene métodos para calcular la velocidad de un ciclista con los parámetros proporcionados (potencia, peso, pendiente, viento...) 