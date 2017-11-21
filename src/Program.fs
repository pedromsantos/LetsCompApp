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