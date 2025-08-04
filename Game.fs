module Game
open Silk.NET.GLFW
open Components

let update (state: World) (delta: float32) (keys: Set<Keys>) =
    state
