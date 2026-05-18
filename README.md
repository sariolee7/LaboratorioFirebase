# Torre de Defensa — Laboratorio Firebase + Unity

Laboratorio que integra un juego de defensa de torre en Unity con Firebase Firestore para recolección de analíticos, y un dashboard web para visualización de datos en tiempo real.

El objetivo principal no es el juego en sí, sino el sistema de datos que lo respalda: la recolección silenciosa de analíticos durante cada sesión, su almacenamiento en Firebase Firestore y su visualización en un dashboard web.

---

## Descripción del juego

Enemigos avanzan en oleadas hacia una base y el jugador los elimina con disparos. El juego termina cuando un enemigo alcanza la base.

### Mecánicas principales

- El jugador apunta con el mouse y dispara mediante `ClickHandler`.
- Al hacer clic en un enemigo, se instancia una `Bullet` dirigida hacia el punto de impacto.
- El enemigo recibe daño cuando la bala colisiona con él.
- Si un enemigo llega a la base, el jugador pierde una vida. Si las vidas llegan a 0, el juego termina.
- Al completar una oleada, `GameManager` espera unos segundos y `EnemySpawner` lanza la siguiente con mayor dificultad.

### Flujo de pantallas

```
1. Pantalla de inicio
   └── El jugador ingresa su nombre/alias

2. Partida
   └── Recolección silenciosa de métricas en memoria
       ├── kills por oleada
       ├── tiempo de reacción por enemigo
       └── duración acumulada

3. Game Over
   ├── Escritura a Firestore (players/{nombre}/sessions/{auto-id})
   ├── Pantalla de resultados con puntaje propio
   └── Botón para abrir el dashboard web
```

---

## Arquitectura del proyecto

El proyecto usa singletons (`Instance`) para facilitar la comunicación entre componentes sin referenciar manualmente cada objeto.

| Script | Responsabilidad |
|--------|----------------|
| `GameManager.cs` | Controlador global: vidas, puntaje, oleadas y ciclo de vida de la partida |
| `EnemySpawner.cs` | Generación de enemigos por oleada y ajuste de dificultad |
| `Enemy.cs` | Movimiento, recepción de daño y colisión con la base |
| `ClickHandler.cs` | Detecta clics del jugador y crea balas con su dirección |
| `Bullet.cs` | Movimiento de bala y aplicación de daño a enemigos |
| `UIManager.cs` | Pantallas de inicio, HUD, game over y actualizaciones de UI |
| `FirestoreManager.cs` | Registra métricas de sesión y guarda datos en Firestore |
| `GameData.cs` | Helper estático para compartir el nombre del jugador entre componentes |

### Estructura de carpetas

```
Assets/
  scripts/
    Firebase/
      FirestoreManager.cs
      GameData.cs
    ScriptBalas/
      Bullet.cs
    GameManager.cs
    EnemySpawner.cs
    Enemy.cs
    ClickHandler.cs
    UIManager.cs
  google-services.json
```

---

## Firebase — Configuración

### Pasos de configuración

1. Ir a [Firebase Console](https://console.firebase.google.com) y crear el proyecto `LaboratorioFirebaseUnity`.
2. Activar **Cloud Firestore** en modo producción.
3. Activar **Realtime Database** 
4. Registrar una app **Android** y una app **Web** (`</>`).
5. Descargar `google-services.json` y colocarlo en `Assets/google-services.json`.
6. Verificar que el **Package Name** de Unity coincida con el registrado en Firebase.

---

## Firebase — Estructura de datos

```
players/
  {playerName}/
    playerName : string
    lastSeen   : timestamp
    sessions/
      {auto-id}/
        playerName      : string
        startTime       : timestamp
        duration        : number
        finalScore      : number
        waveReached     : number
        totalKills      : number
        killsPerWave    : map
        killsPerMinute  : number
        avgReactionTime : number
```

### Esquema de analíticos

**Campos base**

| Campo | Tipo | Justificación |
|-------|------|---------------|
| `playerName` | string | Identifica al jugador y agrupa sus sesiones |
| `startTime` | timestamp | Fecha y hora exacta de inicio de sesión |
| `duration` | number | Duración total en segundos para medir resistencia |
| `finalScore` | number | Puntaje final para el ranking |

**4 métricas de mecánica**

| Campo | Tipo | Justificación |
|-------|------|---------------|
| `waveReached` | number | Mide qué tan lejos llegó el jugador; indica dominio de la dificultad progresiva |
| `totalKills` | number | Volumen total de enemigos eliminados por sesión |
| `killsPerWave` | map | Revela en qué oleada el jugador empieza a tener dificultades |
| `killsPerMinute` | number | Mide la eficiencia y ritmo de combate del jugador |

**1 métrica de comportamiento no trivial**

| Campo | Tipo | Justificación |
|-------|------|---------------|
| `avgReactionTime` | number | Tiempo promedio entre aparición de enemigo y primer disparo. Valores bajos indican mayor agilidad y concentración |

---

## Firebase — Código Unity (C#)

### `GameData.cs`
Clase estática que comparte el nombre del jugador entre componentes:
```csharp
public static class GameData
{
    public static string PlayerName { get; set; }
}
```

### `FirestoreManager.cs`
Maneja toda la comunicación con Firebase:
- `Start()` — inicializa Firestore y se suscribe al evento `OnGameOver`
- `HandleGameOver()` — captura stats del GameManager y llama `SaveSession`
- `SaveSession()` — escribe el documento de sesión en Firestore con todas las métricas
- `ResetSession()` — reinicia variables para una nueva partida
- `OnApplicationQuit()` — guarda datos si el juego se cierra inesperadamente

### Botón para abrir el dashboard
```csharp
dashboardButton.onClick.AddListener(() =>
{
    Application.OpenURL("https://laboratoriofirebaseunity.web.app");
});
```

---

## Dashboard Web
 
Página web desplegada en Firebase Hosting que lee datos reales desde Firestore.
 
**URL:** `https://laboratoriofirebaseunity.web.app`
 
### Visualizaciones
 
| Sección | Tipo | Descripción |
|---------|------|-------------|
| Stats rápidas | Tarjetas | Sesiones totales, mejor puntaje, total kills, oleada máxima, duración promedio |
| Ranking | Tabla | Ordenada por puntaje con medallas para los 3 primeros |
| Distribución de puntajes | Gráfica de barras | Puntaje final por sesión |
| Curva de supervivencia | Gráfica de línea | Duración de las últimas 10 partidas en orden cronológico |
| Kills por minuto | Gráfica de barras | Eficiencia de combate por jugador |
| Tiempo de reacción | Gráfica de línea | Métrica no trivial: agilidad del jugador |
| Distribución por oleada | Gráfica de donut | En qué oleada cae la mayoría de los jugadores |
 
El dashboard incluye un **selector de jugador** que filtra todas las visualizaciones por jugador específico, permitiendo analizar el comportamiento individual.

### Tecnologías

- HTML + CSS + JavaScript vanilla
- [Chart.js](https://www.chartjs.org/) para las gráficas
- Firebase JS SDK v10 para leer Firestore
- Firebase Hosting para el despliegue

### Despliegue

```bash
npm install -g firebase-tools
firebase login

mkdir dashboard
cd dashboard
firebase init hosting
# Seleccionar: Use an existing project → laboratoriofirebaseunity
# Public directory → public
# Single-page app → No
# GitHub deploys → No

firebase deploy
```

---

## Requisitos técnicos

- Unity 2022+ con URP
- Firebase Unity SDK (Firestore)
- Node.js + Firebase CLI
- Cuenta de Google con proyecto Firebase activo

---

## Notas

- Las escrituras a Firestore ocurren solo al finalizar cada sesión, no durante el juego, para no afectar el rendimiento.
- El flag `sessionSaved` evita que una sesión se guarde dos veces si el jugador muere y luego cierra el juego.
- Las colecciones en Firestore se crean automáticamente al escribir el primer documento.
- El dashboard debe abrirse en modo incógnito si alguna extensión de Chrome interfiere con los módulos ES6.
