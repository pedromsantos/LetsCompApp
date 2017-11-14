module LetsCompTests

open Xunit
open FsUnit
open FsCheck
open FsUnit.Xunit
open FsCheck.Xunit
open System.Web
open Microsoft.AspNetCore.Hosting
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

    get client ("/Notes/sharp?Note=" + HttpUtility.UrlEncode (noteName note))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "\"%s\"" (sharp note |> noteName))

[<Property>]
let ``Should flat note`` (note :Note)  =
     use server = new TestServer(createHost())
     use client = server.CreateClient()
 
     get client ("/Notes/flat?Note=" + HttpUtility.UrlEncode (noteName note))
     |> ensureSuccess
     |> readText
     |> should equal (sprintf "\"%s\"" (flat note |> noteName))
     
[<Property>]
let ``Should transpose note by interval`` (note :Note) (interval :Interval) =
     ( not (interval = PerfectOctave) )
         ==> lazy (
                     use server = new TestServer(createHost())
                     use client = server.CreateClient()
                 
                     get client ("/Notes/transpose?Note=" + HttpUtility.UrlEncode (noteName note)
                        + "&Interval=" + (intervalName interval))
                     |> ensureSuccess
                     |> readText
                     |> should equal (sprintf "\"%s\"" ((transpose note interval) |> noteName)))

[<Property>]
let ``Should measure distance in semitones between two notes`` (noteA :Note) (noteB :Note) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()
  
    get client ("/Notes/distance?LowNote=" + HttpUtility.UrlEncode (noteName noteA)
        + "&HighNote=" + HttpUtility.UrlEncode (noteName noteB))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "%i" (measureAbsoluteSemitones noteA noteB))
    
[<Property>]
let ``Should calculate interva between two notes`` (noteA :Note) (noteB :Note) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()
   
    get client ("/Notes/interval?LowNote=" + HttpUtility.UrlEncode (noteName noteA)
         + "&HighNote=" + HttpUtility.UrlEncode (noteName noteB))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "\"%s\"" ((intervalBetween noteA noteB) |> intervalName))