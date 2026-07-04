# Build standalone

Configuracion esperada para la demo:

- Plataforma: Windows Standalone.
- Escena inicial: `Assets/Scenes/MenuInicio.unity`.
- Flujo de escenas: `MenuInicio -> PantallaCarga -> SampleScene`.
- Resolucion base: `960x720`.
- Relacion de aspecto: `4:3`, con barras negras si la pantalla no coincide.

Pasos:

1. Abrir el proyecto con Unity 6.
2. Ir a `File > Build Profiles` o `File > Build Settings`.
3. Seleccionar `Windows`.
4. Confirmar que estan incluidas, en este orden:
   - `Assets/Scenes/MenuInicio.unity`
   - `Assets/Scenes/PantallaCarga.unity`
   - `Assets/Scenes/SampleScene.unity`
5. Usar `Build` y elegir una carpeta fuera del repositorio, por ejemplo `Builds/DoomCityAlpha`.
6. Ejecutar el `.exe` generado.

No subir la carpeta del build al repositorio.
