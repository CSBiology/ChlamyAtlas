module Server.Tests

open Expecto

open Shared
open Server

let server =
    testList "Server" [
        testCase "long aa seq, middle cut out"
        <| fun _ ->
            let maxSequenceLength = 10
            // abc string
            let sequence = "abcdefghijlmnopqrstuvwxyz"
            let acual = FastaReader.trimMiddle0 (sequence, maxSequenceLength)
            let expected = "abcdevwxyz"
            Expect.equal acual expected ""
    ]

let all = testList "All" [ Shared.Tests.shared; server ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all