module LetsComp.App

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions
open Giraffe.Middleware
open Easy.Common
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

let private accident accident =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let noteParameter = ctx.BindQueryString<NoteParameter>()
            return! (noteParameter.Note |> accidentNote accident) next ctx
        }

let private measureDistance notes =
    (measureAbsoluteSemitones (parseNote notes.LowNote) (parseNote notes.HighNote))
    |> json

let private distance =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let notes = ctx.BindQueryString<NoteDistanceParameters>()
            return! (measureDistance notes) next ctx
        }

let private calculatenterval notes =
    (intervalBetween (parseNote notes.LowNote) (parseNote notes.HighNote))
    |> intervalName
    |> json

let private interval =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let notes = ctx.BindQueryString<NoteDistanceParameters>()
            return! (calculatenterval notes) next ctx
        }

let private transposeNote note =
    (transpose (parseNote note.Note) (parseInterval note.Interval))
    |> noteName
    |> json

let private transpose =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let note = ctx.BindQueryString<TransposeParameters>()
            return! (transposeNote note) next ctx
        }

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> json "Hello from LetsComp"
                route "/Notes/flat" >=> accident flat
                route "/Notes/sharp" >=> accident sharp
                route "/Notes/distance" >=> distance
                route "/Notes/interval" >=> interval
                route "/Notes/transpose" >=> transpose
            ]
        setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex : Exception) (logger : ILogger) =
    let report = DiagnosticReport.Generate()
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.\n" + report)
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureLogging(configureLogging)
        .UseAzureAppServices()
        .UseApplicationInsights()
        .Build()
        .Run()
    0