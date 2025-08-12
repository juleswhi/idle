module Game
open Silk.NET.GLFW
open Components

let gravity_update (state: World) (delta: float32) (_: Set<Keys>) =
    state

let update (state: World) (delta: float32) (keys: Set<Keys>) =
    gravity_update state delta keys

