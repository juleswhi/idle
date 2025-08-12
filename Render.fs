module Renderer
open SkiaSharp
open Components

let render (canvas: SKCanvas) (world: World) =
    canvas.Clear SKColors.White

    use paint = new SKPaint()
    paint.IsAntialias <- true

    let renderable_entities =
        world.Components
        |> Map.toSeq
        |> Seq.collect (fun (entity, comps) ->
            let mutable pos = None
            let mutable sprite = None

            for comp in comps do
                match comp with
                | Position p -> pos <- Some p
                | Sprite s -> sprite <- Some s
                | _ -> ()

            match pos, sprite with
            | Some p, Some s -> [| (entity, p, s)|]
            | _ -> [||]
            )

    renderable_entities
    |> Seq.iter (fun (entity, pos, sprite) ->
        match sprite with
        | Texture texId ->
            paint.Color <- SKColors.Pink
            canvas.DrawCircle(SKPoint(float32 pos.X, float32 pos.Y), 10f, paint)

        | Shape (shape, colour) ->
            paint.Color <- colour
            match shape with
            | Rectangle (width, height) ->
                let rect = SKRect(
                    float32 (pos.X - width/2.0),
                    float32 (pos.Y - height/2.0),
                    float32 (pos.X + width/2.0),
                    float32 (pos.Y + height/2.0))
                canvas.DrawRect(rect, paint)

            | Square size ->
                let rect = SKRect(
                    float32 (pos.X - size/2.0),
                    float32 (pos.Y - size/2.0),
                    float32 (pos.X + size/2.0),
                    float32 (pos.Y + size/2.0))
                canvas.DrawRect(rect, paint)

            | Circle radius ->
                canvas.DrawCircle(
                    SKPoint(float32(pos.X - radius/2.0), float32(pos.Y - radius/2.0)),
                    float32 radius,
                    paint)



    )
