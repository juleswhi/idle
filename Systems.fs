module Systems

open Components

let movement_system (world: World) =
    let updated_components_map = // Renamed to avoid confusion with `updated_components` in your snippet
        world.Components
        |> Map.map (fun entity comps ->
            let pos_opt = comps |> List.tryPick (fun comp ->
                match comp with
                | Position p -> Some p
                | _ -> None)
            let vel_opt = comps |> List.tryPick (fun comp ->
                match comp with
                | Velocity v -> Some v
                | _ -> None)

            match pos_opt, vel_opt with
            | Some pos, Some vel ->
                let new_pos: Position = { X = pos.X + vel.X; Y = pos.Y + vel.Y }

                // First, filter out the old Position.
                // Then, prepend the new Position to the *result* of the filter.
                (Position new_pos) :: (comps |> List.filter (function | Position _ -> false | _ -> true))

            | _ -> comps // No change if no position or velocity
        )
    // Create a new world record with the updated components map
    { world with Components = updated_components_map }

