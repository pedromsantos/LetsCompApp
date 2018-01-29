module LetsComp.App

open System

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions
open Giraffe.Middleware

open Easy.Common

open Vaughan.Notes

open LetsComp.Notes
open LetsComp.Chords

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
                route "/Chords/tabify" >=> tabify
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
        .UseAzureAppServices()
        .UseApplicationInsights()
        .ConfigureLogging(configureLogging)
        .Configure(Action<IApplicationBuilder> configureApp)
        .Build()
        .Run()
    0