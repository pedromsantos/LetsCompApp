module LetsCompTests

open Xunit
open FsCheck
open FsUnit.Xunit
open FsCheck.Xunit
open System
open System.Net
open Microsoft.AspNetCore.TestHost
open Vaughan.Domain
open Vaughan.Notes
open Vaughan.Chords
open Vaughan.Guitar
open Vaughan.GuitarTab

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
let ``Should calculate interval between two notes`` (noteA :Note) (noteB :Note) =
    use server = new TestServer(createHost())
    use client = server.CreateClient()

    get client ("/Notes/interval?LowNote=" + WebUtility.UrlEncode (noteName noteA)
         + "&HighNote=" + WebUtility.UrlEncode (noteName noteB))
    |> ensureSuccess
    |> readText
    |> should equal (sprintf "\"%s\"" ((intervalBetween noteA noteB) |> intervalName))

[<Property>]
let ``Should tabify chord`` (root:Note) (quality:ChordQuality) (structure:ChordType) =
    ( quality = Major || quality = Major7 || quality = Minor || quality = Minor7 || quality = Dominant7 ||
      quality = Diminished || quality = Diminished7 || quality = Augmented || quality = Augmented7)
        ==> lazy (

            use server = new TestServer(createHost())
            use client = server.CreateClient()
            let request = "/Chords/tabify?Chord=" +
                          WebUtility.UrlEncode (noteName root) +
                          WebUtility.UrlEncode ((sprintf "%A" quality).Replace("\"", "")) +
                          WebUtility.UrlEncode ((sprintf "%A" structure).Replace("\"", ""))

            let expectedTab =
                match structure with
                | Drop2 -> chord root quality |> toDrop2
                | Drop3 -> chord root quality |> toDrop3
                | _ -> chord root quality
                |> createGuitarChord SixthString
                |> tabify

            get client request
            |> ensureSuccess
            |> readText
            |> should equal (sprintf "%s" expectedTab))