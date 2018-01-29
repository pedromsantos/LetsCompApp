module LetsComp.Chords
open System

open Microsoft.AspNetCore.Http

open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions

open Vaughan.Domain
open Vaughan.Guitar
open Vaughan.GuitarTab
open Vaughan.SpeechToMusic

[<CLIMutable>]
type TabifyParameters =
    {
        Chord: string;
    }

let tabify =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let tabifyParameter = ctx.BindQueryString<TabifyParameters>()
            return! (
                     tabifyParameter.Chord
                     |> parseChord
                     |> createGuitarChord SixthString
                     |> tabify
                     |> text
                    )
                    next ctx
        }