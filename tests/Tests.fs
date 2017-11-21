module LetsCompTests

open Xunit
open FsCheck
open FsUnit.Xunit
open FsCheck.Xunit
open System.Net
open Microsoft.AspNetCore.TestHost
open Vaughan.Domain
open Vaughan.Notes

open AcceptanceTestHelpers

[<Fact>]
let ``Get to root returns hello`` () =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    get client "/"
    |> ensureSuccess
    |> readText
    |> should equal "\"Hello from LetsComp\""

[<Property>]
let ``Should sharp note`` (note :Note)  =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    get client ("/Notes/sharp?Note=" + WebUtility.UrlEncode (noteName note))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "\"%s\"" (sharp note |> noteName))

[<Property>]
let ``Should flat note`` (note :Note)  =
     use server = new TestServer(createHost())
     use client = server.CreateClient()

     get client ("/Notes/flat?Note=" + WebUtility.UrlEncode (noteName note))
     |> ensureSuccess
     |> readText
     |> should equal (sprintf "\"%s\"" (flat note |> noteName))

[<Property>]
let ``Should transpose note by interval`` (note :Note) (interval :Interval) =
     (interval <> PerfectOctave)
         ==> lazy (
                     use server = new TestServer(createHost())
                     use client = server.CreateClient()

                     get client ("/Notes/transpose?Note=" + WebUtility.UrlEncode (noteName note)
                        + "&Interval=" + (intervalName interval))
                     |> ensureSuccess
                     |> readText
                     |> should equal (sprintf "\"%s\"" ((transpose note interval) |> noteName)))

[<Property>]
let ``Should measure distance in semitones between two notes`` (noteA :Note) (noteB :Note) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    get client ("/Notes/distance?LowNote=" + WebUtility.UrlEncode (noteName noteA)
        + "&HighNote=" + WebUtility.UrlEncode (noteName noteB))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "%i" (measureAbsoluteSemitones noteA noteB))

[<Property>]
let ``Should calculate interva between two notes`` (noteA :Note) (noteB :Note) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    get client ("/Notes/interval?LowNote=" + WebUtility.UrlEncode (noteName noteA)
         + "&HighNote=" + WebUtility.UrlEncode (noteName noteB))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "\"%s\"" ((intervalBetween noteA noteB) |> intervalName))