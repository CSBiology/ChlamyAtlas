module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let shared =
    testList "Shared" [
        testCase "Placeholder"
        <| fun _ ->
            Expect.equal 1 1 ""
    ]