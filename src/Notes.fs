module LetsComp.Notes

open Microsoft.AspNetCore.Http

open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions

open Vaughan.Notes
open Vaughan.SpeechToMusic

[<CLIMutable>]
type NoteParameter =
    {
        Note : string
    }

[<CLIMutable>]
type DistanceParameters =
    {
        LowNote  : string;
        HighNote : string
    }

[<CLIMutable>]
type TransposeParameters =
    {
        Note     : string;
        Interval : string
    }

let private accidentNote accident =
     parseNote >> accident >> noteName

let private measureDistance distanceParameters =
    measureAbsoluteSemitones (parseNote distanceParameters.LowNote) (parseNote distanceParameters.HighNote)

let private calculatenterval distanceParameters =
    (intervalBetween (parseNote distanceParameters.LowNote) (parseNote distanceParameters.HighNote))
    |> intervalName
    
let private transposeNote transposeParameters =
    (transpose (parseNote transposeParameters.Note) (parseInterval transposeParameters.Interval))
    |> noteName
    
let accident accident =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let noteParameter = ctx.BindQueryString<NoteParameter>()
            return! (accidentNote accident noteParameter.Note |> json) next ctx
        }

let distance =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let distanceParameters = ctx.BindQueryString<DistanceParameters>()
            return! (measureDistance distanceParameters |> json) next ctx
        }

let interval =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let distanceParameters = ctx.BindQueryString<DistanceParameters>()
            return! (calculatenterval distanceParameters |> json) next ctx
        }

let transpose =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let transposeParameters = ctx.BindQueryString<TransposeParameters>()
            return! (transposeNote transposeParameters |> json) next ctx
        }
