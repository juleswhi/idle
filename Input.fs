module Input
open Silk.NET.GLFW

open Components

let mutable keys_pressed = Set.empty<Keys>

let set_key_callback (glfw: Glfw) window =
    glfw.SetKeyCallback(window, fun _ key _ action _ ->
        match action with
        | InputAction.Press -> keys_pressed <- keys_pressed.Add key
        | InputAction.Release -> keys_pressed <- keys_pressed.Remove key
        | _ -> ()
    ) |> ignore


let input_system (world: World) =
    world

let player_input_system (world: World) : World =
    let player = World.get_player world

    match player with
    | Some p ->
        match World.get_comp_list p world with
        | Some comps ->
            let current_vel_opt = comps |> List.tryPick (function Velocity v -> Some v | _ -> None)

            let x_vel =
                if Set.contains Keys.D keys_pressed then
                    10.0f
                else if Set.contains Keys.A keys_pressed then
                    -10.0f
                else
                    0.0f

            let y_vel =
                if Set.contains Keys.S keys_pressed then
                    10.0f
                else if Set.contains Keys.W keys_pressed then
                    -10.0f
                else
                    0.0f

            let new_vel = { X = float x_vel; Y = float y_vel }

            let velocity_changed =
                match current_vel_opt with
                | Some v when v.X = new_vel.X && v.Y = new_vel.Y -> false
                | _ -> true

            if velocity_changed then
                World.update_comp p (Velocity new_vel) world
            else
                world
        | None -> world
    | None -> world
