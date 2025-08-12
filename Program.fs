module Program

open FSharp.NativeInterop

open Silk.NET.GLFW
open SkiaSharp

open System.Diagnostics

open Input
open Systems
open Components
open Game

#nowarn "9"

let width, height = 800, 600
let target_fps= 60f
let frame_time = 1f / target_fps

let glfw = Glfw.GetApi()

if not(glfw.Init()) then failwith "Initialisation failed"

let window = glfw.CreateWindow(width, height, "idle", NativePtr.ofNativeInt 0n, NativePtr.ofNativeInt 0n)
if NativePtr.isNullPtr window  then
    failwith "Window creation failed"

glfw.MakeContextCurrent window
glfw.SwapInterval 1

set_key_callback glfw window

let grGlInterface = GRGlInterface.Create(fun name -> glfw.GetProcAddress name)

if not(grGlInterface.Validate()) then failwith "Invalid GRGLInterface"

let grContext = GRContext.CreateGl grGlInterface
let frame_buffer_info = new GRGlFramebufferInfo(0u, SKColorType.Rgba8888.ToGlSizedFormat())
let render_target = new GRBackendRenderTarget(width, height, 1, 0, frame_buffer_info)

let stopwatch = Stopwatch.StartNew()
let mutable lag = 0.0
let mutable accum = 0.0

let initial_world =
    let w, player = World.empty |> World.new_entity
    let w, circle = w |> World.new_entity
    w
    |> World.add_comp player (Position {X = 25; Y = 25})
    |> World.add_comp player (Velocity {X = 0; Y = 0})
    |> World.add_comp player (Sprite (Shape (Rectangle (50.0, 50.0), SKColors.Red)))
    |> World.add_comp player (Health {Current = 100; Max = 100 })
    |> World.add_comp player Player
    |> World.add_comp player Gravity
    |> World.add_comp circle (Position {X = 200; Y = 200})
    |> World.add_comp circle (Sprite (Shape (Circle 50.0, SKColors.Black)))

let systems: (World -> World) list = [movement_system; player_input_system]
let mutable world = initial_world

printfn "Initialised"

while not(glfw.WindowShouldClose window) do
    let current_time = stopwatch.Elapsed.TotalSeconds
    let elapsed = current_time - accum
    accum <- current_time
    lag <- lag + elapsed

    glfw.PollEvents()

    while lag >= float frame_time do
        if keys_pressed.Contains Keys.Q then
            glfw.SetWindowShouldClose(window, true)

        world <- update world (float32 frame_time) keys_pressed
        lag <- lag - float frame_time

    use surface = SKSurface.Create(grContext, render_target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
    Renderer.render surface.Canvas world

    world <- systems
    |> List.fold (fun w s -> s w) world

    surface.Canvas.Flush()

    glfw.SwapBuffers window

    let render_time = stopwatch.Elapsed.TotalSeconds - current_time
    let sleep_time = max 0 (int ((float frame_time - float render_time) * float 1000f))
    if sleep_time > 1 then
        System.Threading.Thread.Sleep(int sleep_time)

grContext.Dispose()
glfw.DestroyWindow window
glfw.Terminate()
