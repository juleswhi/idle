open FSharp.NativeInterop

open Silk.NET.GLFW
open SkiaSharp

open System.Diagnostics

#nowarn "9"

type GameState = {
    PlayerX: float32
    PlayerY: float32
}

let width, height = 800, 600
let target_fps= 60f
let frame_time = 1f / target_fps

let glfw = Glfw.GetApi()

if not(glfw.Init()) then failwith "Initialisation failed"

let window = glfw.CreateWindow(width, height, "test", NativePtr.ofNativeInt 0n, NativePtr.ofNativeInt 0n)
glfw.MakeContextCurrent window

let mutable game_state = {
    PlayerX = float32 100
    PlayerY = float32 100
}

let mutable keys_pressed = Set.empty<Keys>

glfw.SetKeyCallback(window, fun _ key _ action _ ->
    match action with
    | InputAction.Press -> keys_pressed <- keys_pressed.Add key
    | InputAction.Release -> keys_pressed <- keys_pressed.Remove key
    | _ -> ()
) |> ignore

let grGlInterface = GRGlInterface.Create(fun name -> glfw.GetProcAddress name)

if not(grGlInterface.Validate()) then failwith "Invalid GRGLInterface"

let grContext = GRContext.CreateGl grGlInterface
let grGlFrameBufferInfo = new GRGlFramebufferInfo(0u, SKColorType.Rgba8888.ToGlSizedFormat())
let grGlBackendRenderTarget = new GRBackendRenderTarget(width, height, 1, 0, grGlFrameBufferInfo)

let render (canvas: SKCanvas) (state: GameState) =
    canvas.Clear SKColors.White
    use player_paint = new SKPaint(Color = SKColors.Blue, IsAntialias = true)
    canvas.DrawCircle(state.PlayerX, state.PlayerY, 30f, player_paint)


let update(state: GameState) (delta: float32) (keys: Set<Keys>) =
    let mutable new_state = state
    new_state

let stopwatch = Stopwatch.StartNew()
let mutable lag = 0.0
let mutable accum = 0.0

while not(glfw.WindowShouldClose window) do
    let current_time = stopwatch.Elapsed.TotalSeconds
    let elapsed = current_time - accum
    accum <- current_time
    lag <- lag + elapsed

    glfw.PollEvents()

    while lag >= float frame_time do
        game_state <- update game_state (float32 frame_time) keys_pressed
        lag <- lag - float frame_time

    use surface = SKSurface.Create(grContext, grGlBackendRenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
    render surface.Canvas game_state

    surface.Canvas.Flush()

    glfw.SwapBuffers window

    let render_time = stopwatch.Elapsed.TotalSeconds - current_time
    let sleep_time = max 0 (int ((float frame_time - float render_time) * float 1000f))
    if sleep_time > 1 then
        System.Threading.Thread.Sleep(int sleep_time)

grContext.Dispose()
glfw.DestroyWindow window
glfw.Terminate()
