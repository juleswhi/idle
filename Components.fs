module Components

open SkiaSharp

type Shape =
    | Rectangle of width : float * height : float
    | Circle of radius : float
    | Square of width : float

type Entity = int
type Position = { X: float; Y: float }
type Velocity = { X: float; Y: float }
type Health = { Current: int; Max: int }
type Sprite =
    | Texture of string
    | Shape of Shape * SKColor

type Component =
    | Position of Position
    | Velocity of Velocity
    | Health of Health
    | Sprite of Sprite
    | Player
    | Gravity

type World = {
    Entities: Set<Entity>
    Components: Map<Entity, Component list>
    NextEntityId: Entity
}

type System = World -> World


module World =
    let empty = {
        Entities = Set.empty
        Components = Map.empty
        NextEntityId =  0
    }

    let new_entity world =
        let e = world.NextEntityId
        { world with
            Entities = Set.add e world.Entities
            Components = Map.add e [] world.Components
            NextEntityId = e + 1
        }, e

    let add_comp entity comp world =
        match Map.tryFind entity world.Components with
        | Some comps ->
            let updated_comps = comp :: comps
            { world with Components = Map.add entity updated_comps world.Components }
        | None -> world

    let tryGetComponent<'T> (comp: obj) : 'T option =
        match comp with
        | :? 'T as c -> Some c
        | _ -> None

    let get_comp<'T> world =
        world.Components
        |> Map.toSeq
        |> Seq.choose (fun (entity, comps) ->
            comps
            |> List.tryPick (fun comp ->
                match tryGetComponent<'T> comp with
                | Some c -> Some(entity, c)
                | None -> None
            )
        )

    let get_player world =
        world.Components
        |> Map.toSeq
        |> Seq.tryPick (fun (entity, comps) ->
            if List.exists (function Player -> true | _ -> false) comps then
                Some entity
            else
                None
            )

    let get_comp_list (entity: Entity) (world: World) : Component list option =
        Map.tryFind entity world.Components


    let update_comp (entity: Entity) (comp: Component) (world: World) : World =
        match Map.tryFind entity world.Components with
        | Some comps ->
            // Remove existing component of the same type, then add the new one
            let filtered_comps =
                comps
                |> List.filter (fun c ->
                    match c, comp with // Pattern match on both old and new component
                    | Position _, Position _ -> false
                    | Velocity _, Velocity _ -> false
                    | Health _, Health _ -> false
                    | Sprite _, Sprite _ -> false
                    | Player, Player -> false
                    | _ -> true
                )
            let updated_comps = comp :: filtered_comps
            { world with Components = Map.add entity updated_comps world.Components }
        | None -> world



