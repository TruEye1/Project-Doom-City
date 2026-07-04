# Doom City

Doom City es una demo Alpha de un videojuego Beat 'em up 2D con estética arcade/pixel art, desarrollado en Unity 6 y C#. El jugador controla a Chris, avanza por una calle urbana, enfrenta oleadas de enemigos y completa el tramo disponible de la Alpha.

El proyecto corresponde a un trabajo estudiantil de Programación de Videojuegos y busca consolidar una base jugable clara para futuras expansiones: nuevos enemigos, nuevos niveles, sistema de monedas, temporizador por nivel, puntuación, HUD completo y más contenido narrativo.

## Estado Actual

La demo ya cuenta con un loop jugable completo:

- Menú de inicio con Jugar, Configuración y Salir.
- Pantalla de carga asincrónica.
- Escena principal `SampleScene`.
- Movimiento, carrera, salto visual y combo de cuatro golpes para Chris.
- IA enemiga con persecución, ataque, daño, knockdown, invulnerabilidad temporal y muerte.
- Oleadas progresivas de enemigos: 2 al inicio del avance, 2 a mitad del escenario y 4 cerca del final.
- HUD con barras de vida visuales para jugador y enemigo.
- Menú de pausa, reinicio de nivel y retorno al menú principal.
- Game Over al morir el jugador.
- Mensaje de cierre al completar la Alpha.
- Relación de aspecto 4:3 para mantener el formato arcade.
- Límites de escenario corregidos para evitar que jugador y enemigos salgan del área jugable.
- Efectos de sonido de impacto para golpes del jugador y enemigos.

## Controles

| Acción | Tecla |
| --- | --- |
| Mover a Chris | `WASD` |
| Correr | `Left Shift` |
| Saltar | `Espacio` |
| Golpear / continuar combo | `J` |
| Pausar / reanudar | `Escape` |
| Navegar menús | Mouse |

## Tecnología

- Motor: Unity 6.
- Lenguaje: C#.
- Render: URP 2D.
- Input: Unity Input System.
- Animación: Animator Controller clásico y Animation Events.
- Física: Rigidbody2D Kinematic con límites de escenario por código.
- UI: Canvas, TextMeshPro, sliders/fill visuales y menús runtime.
- Plataforma objetivo: Windows PC.

## Estructura Principal

```text
Assets/
  Scenes/
    MenuInicio.unity
    PantallaCarga.unity
    SampleScene.unity
  Scripts/
    PlayerController.cs
    EnemigoIA.cs
    AtaqueJugador.cs
    AtaqueEnemigo.cs
    SaludJugador.cs
    StageBounds2D.cs
    ArcadeAspectRatioController.cs
    Spawning/
      EnemyWaveSpawner.cs
    UI/
      GameHUD.cs
      HealthBarUI.cs
      GameOverMenuView.cs
      AlphaCompletionView.cs
      ExitConfirmationView.cs
```

## Documento de Visión

El documento de visión actualizado del proyecto está disponible en:

[Documento.de.Vision.de.Proyecto.1.docx](Documento.de.Vision.de.Proyecto.1.docx)

Incluye el estado actual de la demo Alpha, sistemas implementados, criterios de validación, riesgos pendientes y roadmap de futuras mejoras.

## Cómo Abrir el Proyecto

1. Clonar el repositorio.
2. Abrir la carpeta del proyecto con Unity 6.
3. Cargar la escena `Assets/Scenes/MenuInicio.unity`.
4. Ejecutar Play Mode desde el menú inicial.

## Build Standalone

La configuración esperada para crear un ejecutable de Windows se documenta en:

[BUILD_STANDALONE.md](BUILD_STANDALONE.md)

Flujo de escenas esperado:

```text
MenuInicio -> PantallaCarga -> SampleScene
```

Resolución base:

```text
960x720, relación 4:3
```

## Roadmap Futuro

- Nuevos enemigos con comportamientos diferenciados.
- Nuevos niveles urbanos y transición entre escenarios.
- Jefes con patrones de ataque y fases.
- Sistema de recolección de monedas.
- Sistema de puntuación y ranking.
- Temporizador por nivel.
- HUD completo con monedas, tiempo, puntaje, combo y vidas/continues.
- Más pickups: comida, dinero, armas temporales y bonus.
- Mejoras de feedback: partículas, VFX de impacto, sonidos por tipo de golpe y pulido de animaciones.
- Guardado local de puntajes máximos y progreso.
- Configuración más completa de audio, pantalla y controles.

## Créditos

Creado por Jeremy Guajardo, alumno de Ingeniería Informática.

Proyecto estudiantil en desarrollo. La Alpha representa una base jugable y técnica para futuras actualizaciones.
