# Intervals Specifications
## Introduction
Servicio que se encarga de detectar intervalos de potencia en los que esta se mantiene constante. 
Consiste en una función que devolverá tanto los intervalos como los sprints encontrados en la lista de puntos ([`FitnessData`](../../SessionReader/SessionReader.Core/Models/FitnessData.cs)) que se le pase como parámetro. Para más información sobre los sprints, leer el documento de [especificaciones de sprints](SprintSpecifications.md).

Se define como intervalo un cantidad de tiempo en el que la potencia se mantiene estable dentro de unos límites establecidos. Un intervalo puede estar compuesto por varios intervalos de duración más corta.

## Limitations
A continuación, las principales limitaciones del servicio:
1- El tiempo mínimo del intervalo es de 30 segundos. Todo lo que esté por debajo de este tiempo se considerará un sprint (si cumple los requisitos del [servicio de sprints](SprintSpecifications.md)), o un intervalo nulo.
2- No todo tiene que ser un intervalo. Habrá espacios de tiempo en los que la potencia sea inestable (por ejemplo, un criterium en el que se arranca y se frena constantemente) o demasiado baja como para que merezca la pena tenerlo en cuenta; estos intervalos se considerarán intervalos nulos.

## Process description
El punto clave para la búsqueda de intervalos es el cálculo de la potencia media, la desviación estándar (STD) y el rango de potencia máxima y mínima de los últimos X segundos en cada punto del recorrido (ver modelo [`AveragePowerModel`](../../SessionAnalyzer/SessionAnalyzer.Core/Services/Intervals/AveragePowerModel.cs)). Con estos datos se suaviza la potencia de la actividad durante el recorrido y facilita la búsqueda de intervalos. Estos datos se calcularán para 3 diferentes tiempos, de esta forma se facilitará encontrar intervalos de diferentes tiempos:

* **Short**: intervalos de una duración corta, tiempo mínimo: 30 segundos. Se calcularán con intervalos de tiempos de 10 segundos.
* **Medium**: intervalos de una duración media, tiempo mínimo: 4 minutos. Se calcularán con intervalos de tiempos de 30 segundos.
* **Long**: intervalos de una duración larga, tiempo mínimo: 10 minutos. Se calcularán con intervalos de tiempos de 60 segundos.

> Nota: la agrupación por la duración de los intervalos no tiene unos límites máximos fijos, por lo tanto, un intervalo de 20 minutos podría detectarse en la fase de detección de intervalos cortos, aunque sería difícil en una actividad real. Esta agrupación solo sirve para facilitar el trabajo al algoritmo y definir mejor las pruebas que se vayan a hacer, la función que devuelva dichos intervalos no hará ninguna referencia a estos grupos. 

> Nota: El problema que puede venir del suavizado de la curva es la pérdida de los sprints o la alteración de los intervalos debido a los sprints; es por ello que se ejecutará el [servicio de sprints](SprintSpecifications.md) antes de pasar al proceso de búsqueda de intervalos.

Con esto, el proceso completo quedaría algo así:
1. Ejecución del servicio de sprints, con la eliminación de estos de la potencia de la actividad.
2. Cálculo de potencias medias, desviaciones y rangos de la actividad.
3. Búsqueda de intervalos usando los datos calculados para cada una de las agrupaciones de tiempos. Se realizará un refinamiento del intervalo cuando este se de por finalizado.
4. Limpieza final e integración de intervalos dentro de otros, gestión de colisiones.

### Intervals search
Para la búsqueda de intervalos se utilizarán hasta tres tipos de datos:
1. La relación STD-Media en el periodo. Una relación baja significa que el periodo está siendo estable.
2. El rango del periodo a analizar (diferencia entre el valor máximo y el mínimo), en porcentaje relativo a la media del periodo. Un rango bajo significa que no hay grandes picos en el periodo.
3. Desviación de la media reciente respecto a la referencia (media del intervalo), en porcentaje relativo a la media de la referencia. Una desviación baja significa que la media del periodo se mantiene cerca de la media de referencia.

Para la **detección del arranque** se utilizarán la relación STD-Media y el rango del periodo, los valores aceptable vendrán como parámetro en la llamada de la función o, en caso de que no llegue como parámetro, se utilizarán los valores por defecto establecidos en la clase [`IntervalSearchValues`](../SessionAnalyzer.Core/Constants/IntervalSeachValues.cs). El cumplimiento de estos requisitos durante el tiempo mínimo establecido ([`IntervalTimes`](../SessionAnalyzer.Core/Constants/IntervalTimes.cs), también configurable con parámetros de entrada) significaría el posible comienzo de un intervalo.

Para la **comprobación de la continuidad del intervalo** se utilizará la relación STD-Media y la desviación de la media reciente. Cuanto mayor sea el tiempo del intervalo, mayor será la desviación que se acepte; de esta forma se adminitrán mayores fluctuaciones en los intervalos de larga duración, pero no en los de corta. El cumplimiento de estos requisitos significa la continuidad del intervalo, el no cumplimiento durante el tiempo establecido en [`IntervalTimes`](../SessionAnalyzer.Core/Constants/IntervalTimes.cs), el fin de este.

### Refinement
Se hace un refinamiento de los límites de los intervalos (inicio y final) a través de un "zoom" progresivo desde las medias calculadas hasta los valores reales. Se buscará el punto que se desvíe un 15% de la referencia calculada. Lógicamente, se buscará en los límites del intervalo, no en mitad de este.

En los intervalos cortos se empezará mirando desde las medias de 10 segundos, en los intervalos medios desde las medias de 30 segundos, y en los intervalos largos en las medias de 60 segundos. Cuando se encuentre el punto en el que la media se desvíe ese 15%, se aumentará el zoom hasta llegar a los datos de potencia reales y encontrar el punto exacto en el que empieza o acaba el intervalo.

### Collisions
Dado que se detectan los intervalos de 3 formas diferentes, es muy probable que los intervalos detectados no sean los mismos, incluso que haya colisiones de tiempos entre ellos. Se considera que dos intervalos colisionan cuando solo uno de los dos extremos del espacio de tiempo que ocupa un intervalo (inicio o final) está dentro del espacio que ocupa el otro intervalo. En caso de que tanto el inicio como el final estuviesen dentro del otro intervalo, se consideraría un sub-intervalo.

Las colisiones se gestionan de dos diferentes maneras:
1- Unión entre intervalos. En caso de que sean intervalos de potencia media parecida, se hará una unión de ambos.
2- División del intervalo de menor duración. En caso de que la potencia media no sea parecida, el intervalo de menor duración se dividirá en dos para evitar la colisión. Luego se verá si los intervalos generados son válidos o no.

### Integration
Tal y como se ha indicado al principio: un intervalo puede estar compuesto por varios sub-intervalos de duración más corta. Siguiendo el modelo de clase [`Interval`](../SessionAnalyzer.Core/Models/Interval.cs), se guardarán dentro del intervalo de mayor duración los de menor duración. Por lo tanto, aunque la función solo devuelva los principales intervalos, se podrán encontrar el resto de los intervalos dentro de los principales. 

> Nota: Un sub-intervalo puede contener sus propios sub-intervalos.

## Structure
Para el correcto funcionamiento de este servicio se utilizarán los siguientes elementos:
1. Modelos:
    1. [`Interval`](../SessionAnalyzer.Core/Models/Interval.cs): Modelo público con la información sobre un intervalo.
    2. [`AveragePowerModel`](../SessionAnalyzer.Core/Services/Intervals/AveragePowerModel.cs): Modelo propio del servicio que sirve para guardar los datos calculados (potencia media, desviación, rango...).
2. Repositorio estático [`IntervalRepository`](../SessionAnalyzer.Core/Services/Intervals/IntervalRepository.cs), que sirve para alamcenar la información que vayan a compartir diferentes clases del servicio. Se resetearía cada vez que se vaya a hacer un nuevo cálculo.
3. Servicios:
    1. [`IntervalsService`](../SessionAnalyzer.Core/Services/Intervals/IntervalsService.cs): Clase principal de la búsqueda de intervalos. Se encarga de llevar la lógica principal del proceso.
    2. [`SprintService`](../SessionAnalyzer.Core/Services/Intervals/SprintService.cs): Servicio estático de la búsqueda de sprints. Se encarga de buscarlos y eliminarlos de los datos de potencia (así facilita la búsqueda de intervalos).
    3. [`AveragePowerCalculator`](../SessionAnalyzer.Core/Services/Intervals/AveragePowerCalculator.cs): Servicio estático que se encarga del cálculo de toda la información necesaria (medias, desviaciones, rangos...) para seguir con la lógica de búsqueda.


## Intervals To Dos
* Add async method with multiple tasks for the interval detection. One task for each interval time span (short, medium or long).
* Before spitting an interval when a collision occurs, check if they can be merged, creating a new longer interval.
* Add sprints to final return.
* Eliminar el repositorio IntervalRepository.