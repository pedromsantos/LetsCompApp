module AcceptanceTestHelpers

open System
open System.IO
open System.Net.Http

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost

let createHost() =
    WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(Action<IApplicationBuilder> LetsComp.App.configureApp)

let runTask task =
    task
    |> Async.AwaitTask
    |> Async.RunSynchronously

let get (client : HttpClient) (path : string) =
    path
    |> client.GetAsync
    |> runTask

let createRequest (method : HttpMethod) (path : string) =
    let url = "http://127.0.0.1" + path
    new HttpRequestMessage(method, url)

let makeRequest (client : HttpClient) (request : HttpRequestMessage) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()
    request
    |> client.SendAsync
    |> runTask

let ensureSuccess (response : HttpResponseMessage) =
    if not response.IsSuccessStatusCode
    then response.Content.ReadAsStringAsync() |> runTask |> failwithf "%A"
    else response

let readText (response : HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> runTask