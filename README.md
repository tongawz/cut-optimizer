## cut-optimizer
![20250417013125_1](https://github.com/user-attachments/assets/efe4ac70-3abc-4c80-9911-48f5a89c6ca2)
> Se trata de una herramienta pensada para optimización de cortes en matrices rectangulares. Para aprovechar al máximo el material disponible. Por ejemplo corte de CNC en carpintería.

## 🔍 Evaluación de combinaciones (algoritmo híbrido)

Una vez generadas todas las combinaciones posibles, se ordenan de forma descendente por área total.

### 🧠 Algoritmo de búsqueda:

1. **Inicio**: se ordenan todas las combinaciones por área descendente.
2. Se evalúa la **combinación del medio** del conjunto actual:
   - **Si entra en el panel base**:
     - Se guarda como mejor solución hasta el momento.
     - Se **podan todas las combinaciones con área menor o igual**.
     - Se repite el proceso sobre el nuevo subconjunto.
   - **Si no entra**:
     - Se realiza una **búsqueda lineal hacia abajo** (desde el área más pequeña hacia arriba) hasta encontrar una combinación válida.
     - En cuanto se encuentra una, se actualiza como mejor y se eliminan combinaciones con área menor o igual.
3. El proceso continúa mientras existan combinaciones por evaluar y aún no se haya hallado una solución óptima.

## ⚡ Fallback paralelo con poda

Si muchas combinaciones consecutivas fallan (más del **1% del total**), el sistema activa un **modo paralelo de evaluación con poda**:

- Se procesan múltiples combinaciones en paralelo (usando la **mitad de los cores disponibles**).
- En cuanto una combinación válida y mejor es encontrada, se **detiene el resto de las tareas** (`state.Stop()`).
- Luego se **eliminan combinaciones con área menor o igual** a la mejor obtenida.

Este mecanismo acelera considerablemente los casos donde hay muchas combinaciones inútiles.


## 📦 Resultado final

Una vez encontrada la mejor combinación posible para el panel actual:

- Se agregan los paneles empacados al resultado (`PackedBasePanel`).
- Se eliminan esos paneles del conjunto disponible.
- Se repite el proceso con los paneles restantes, generando tantos paneles base como sea necesario para empacar todos los paneles.


## ℹ️ Nota sobre la opción "Permitir rotación de paneles"

Este sistema genera todas las posibles combinaciones de paneles que se pueden ubicar dentro de un panel base, sin exceder su superficie.  
La cantidad de combinaciones posibles **crece exponencialmente** según dos factores:

- La cantidad total de paneles disponibles
- Si se permite o no rotar los paneles (90°)

A continuación se presentan las **fórmulas** y un **ejemplo comparativo** entre ambas opciones.

## 🔒 Sin rotación

Cuando la opción "Permitir rotación" está **desactivada**, cada panel tiene 2 posibles estados:

- No se incluye
- Se incluye con su orientación original

### 📀 Fórmula teórica:

Si hay `n` paneles disponibles:

```
Total = 2^n - 1
```

> (Se resta 1 para excluir el conjunto vacío, ya que no tiene sentido evaluar una combinación sin paneles.)

---

## 🔁 Con rotación

Cuando la opción "Permitir rotación" está **activada**, algunos paneles pueden aparecer dos veces: una en su forma original y otra rotada (siempre que ambas versiones quepan en el panel base).

Por lo tanto:

- Los paneles **no rotables** tienen 2 estados: no usar, o usar original.
- Los paneles **rotables** tienen 3 estados: no usar, usar original o usar rotado.

### 📀 Fórmula teórica:

Si hay:
- `f` paneles **no rotables**
- `r` paneles **rotables**

Entonces:

```
Total = (2^f) * (3^r) - 1
```

> Nuevamente, se resta 1 para eliminar el conjunto vacío.

---

## 📊 Ejemplo comparativo

Supongamos que hay 11 paneles en total:
- 6 **no rotables**
- 5 **rotables**

### 🔒 Sin rotación:

```
Total = 2^11 - 1 = 2048 - 1 = 2047 combinaciones
```

### 🔁 Con rotación:

```
Total = (2^6) * (3^5) - 1 = 64 * 243 - 1 = 15.552 - 1 = 15.551 combinaciones
```
## 📊 Resultados comparativos

| Con rotación | Paneles | Rotables | No rotable | Paneles bases | Tiempo (s) | Porcentaje de uso (primer panel base)        |
|--------|--------|-----------|----------------|--------|----------|---------------------|
| False  | 1      | 1         | 0              | 1      | 0.08     | 33.27%              |
| True   | 1      | 1         | 0              | 1      | 0.00     | 33.27%              |
| False  | 2      | 1         | 1              | 1      | 0.00     | 55.19%              |
| True   | 2      | 1         | 1              | 1      | 0.00     | 55.19%              |
| False  | 3      | 2         | 1              | 1      | 0.32     | 68.27%              |
| True   | 3      | 2         | 1              | 1      | 5.49     | 68.27%              |
| False  | 4      | 3         | 1              | 2      | 3.22     | 68.27%              |
| True   | 4      | 3         | 1              | 1      | 148,48   | 83.22%              |

---

## ⚠️ Consideraciones

- **No todos los paneles pueden rotarse**: Si una rotación provoca que el panel exceda el ancho o alto permitido, se **descarta automáticamente**.
- A medida que aumenta la cantidad de paneles **rotables**, el número de combinaciones posibles crece **mucho más rápido** que en el caso sin rotación.
- Esto puede implicar un tiempo de procesamiento considerablemente mayor.


---

## 🙏 Agradecimientos

Quiero expresar mi sincero agradecimiento a la librería [**RectpackSharp**](https://github.com/ThomasMiz/RectpackSharp) de **ThomasMiz**, que fue fundamental en la implementación del sistema de packing de paneles en este proyecto. Gracias a su excelente trabajo, pude integrar de manera eficiente la solución de packing en el sistema.

Puedes consultar la librería aquí: [RectpackSharp en GitHub](https://github.com/ThomasMiz/RectpackSharp)
