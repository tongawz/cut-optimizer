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

---

## ⚠️ Consideraciones

- **No todos los paneles pueden rotarse**: Si una rotación provoca que el panel exceda el ancho o alto permitido, se **descarta automáticamente**.
- A medida que aumenta la cantidad de paneles **rotables**, el número de combinaciones posibles crece **mucho más rápido** que en el caso sin rotación.
- Esto puede implicar un tiempo de procesamiento considerablemente mayor.

---

