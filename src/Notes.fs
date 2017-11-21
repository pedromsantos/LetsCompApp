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
type NoteDistanceParameters =
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
     parseNote >> accident >> noteName >> json

let private measureDistance notes =
    (measureAbsoluteSemitones (parseNote notes.LowNote) (parseNote notes.HighNote))
    |> json


let private calculatenterval notes =
    (intervalBetween (parseNote notes.LowNote) (parseNote notes.HighNote))
    |> intervalName
    |> json

let private transposeNote note =
    (transpose (parseNote note.Note) (parseInterval note.Interval))
    |> noteName
    |> json

let accident accident =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let noteParameter = ctx.BindQueryString<NoteParameter>()
            return! (noteParameter.Note |> accidentNote accident) next ctx
        }

let distance =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let notes = ctx.BindQueryString<NoteDistanceParameters>()
            return! (measureDistance notes) next ctx
        }

let interval =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let notes = ctx.BindQueryString<NoteDistanceParameters>()
            return! (calculatenterval notes) next ctx
        }

let transpose =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let note = ctx.BindQueryString<TransposeParameters>()
            return! (transposeNote note) next ctx
        }
