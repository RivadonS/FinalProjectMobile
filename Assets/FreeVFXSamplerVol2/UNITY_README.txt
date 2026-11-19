Using these effects in Unity
============================

T_anim.png is an animated PNG. Unity reads only its first frame, so dragging
T_anim.png into a scene gives you a still image. That file is here for web and
UI use. The animation lives in the numbered frames and in T_sheet.png.


Start with the demo scene
-------------------------
Demo/<pack name> Demo.unity opens the whole pack laid out on a ground plane,
every effect playing at 24 fps with its name beside it. Press Play to watch
them run, or just look at the objects in the Hierarchy to see how each one is
set up: a SpriteRenderer, an Animator, and an additive material. Copy any of
them straight into your own scene.

These are 2D sprite animations, drawn as flat frames and shown in 3D on
camera-facing quads. There are no meshes or particle systems in the pack.


1. Frame sequence - simplest
----------------------------
  * Open <effect>/<variant>/frames in the Project window.
  * Select T_001.png through T_024.png: click the first, shift-click the last.
  * Drag all 24 into the Scene view together.
  * Unity asks where to save an Animation clip. Save it, and you get a
    GameObject with a SpriteRenderer and an Animator already playing.
  * Open Window > Animation and set the Sample Rate to 24. Unity defaults new
    clips to 12, which plays these at half speed.
  * For a one-shot effect, select the clip and uncheck Loop Time.


2. Spritesheet - lighter at runtime
-----------------------------------
  * T_sheet.png arrives already sliced into its 24 frames. Click the arrow on
    the texture in the Project window to expand them.
  * Select <variant>_0 through <variant>_23 and drag them into the Scene.
  * Same result as above, but every frame comes from one texture, so the
    renderer batches them.


3. Particle System - for VFX work
---------------------------------
  * Renderer > Material: an additive particle material, see below.
  * Texture Sheet Animation: Tiles X 5, Y 5, Frames 0 to 23,
    Time Mode Lifetime, using T_sheet.png.


Making them look right
----------------------
These are additive effects, drawn on black with their alpha taken from their
own brightness. Unity's default Sprites/Default material is alpha blended, so
against a dark scene they come out flat and washed out.

Assign a material using one of these shaders instead:

  Built-in render pipeline : Legacy Shaders/Particles/Additive
  URP and HDRP             : Universal Render Pipeline/Particles/Unlit,
                             with Blending Mode set to Additive

Normal alpha blending still works and simply reads a little flatter.


Import settings
---------------
Every texture already carries the settings it wants: Sprite type, Alpha Is
Transparency on, no compression so the gradients do not band, clamped
wrapping, and 384 pixels per unit, which makes a 768 px effect two world
units, roughly the height of a humanoid rig. T_sheet.png is pre-sliced into its
24 frames. Nothing needs re-slicing or reconfiguring by hand.

Authored at 24 fps, 24 frames per effect.
