module FastaReader

open System
open System.IO
open Shared

open System.Text
open System.Text.RegularExpressions

let regexPattern = @"\|(?<uid>[\S]+?)\|"

let regex = Regex(regexPattern)

// if you run this locally change this value to allow parsing more values at once
let private maxCount = 9999999



/// Cuts out middle aa seq part. Target sequences are mostly found at start or end of sequence. So this is the best way to improve performance for super large sequences.
let trimMiddle0(sequence: string, maxCount) =
    let length = sequence.Length
    if length > Constants.MaxSequenceLength then
        let half = length / 2
        let diff = length - Constants.MaxSequenceLength
        let halfDiff = diff / 2
        let first = sequence.Substring(0, half-halfDiff)
        let last = sequence.Substring(length - half + halfDiff)
        first + last
    else
        sequence

let trimMiddle(sequence: string) =
    trimMiddle0(sequence, Constants.MaxSequenceLength)
    
let private read (reader:TextReader) =
    let mutable noNameIterator = 0
    let mutable iterator = 0
    let rec readRecords (acc: DataInputItem list) (currentHeader: string) (currentSeq: string) =
        let nextLine = reader.ReadLine()
        match nextLine with
        | null | _ when (iterator >= maxCount) || (isNull nextLine) ->
            let trimmedSeq = trimMiddle currentSeq
            List.rev ({ Header = currentHeader; Sequence = trimmedSeq } :: acc)
        | line when line.StartsWith(">!") ->
            iterator <- iterator + 1
            noNameIterator <- noNameIterator + 1
            let newHeader = sprintf "Unknown%i" noNameIterator
            let line' = line.Substring(2).Trim()
            if currentHeader <> "" && currentSeq <> "" then
                let trimmedSeq = trimMiddle currentSeq
                readRecords ({ Header = currentHeader; Sequence = trimmedSeq } :: acc) newHeader line'
            else
                readRecords acc newHeader line'
        | line when line.StartsWith(">") ->
            let newHeader =
                let raw = line.Substring(1)
                let m = regex.Match(raw)
                match m.Success with
                | true -> m.Groups.["uid"].Value
                | false -> raw
            iterator <- iterator + 1
            if currentHeader <> "" && currentSeq <> "" then
                let trimmedSeq = trimMiddle currentSeq
                readRecords ({ Header = currentHeader; Sequence = trimmedSeq } :: acc) newHeader ""
            else
                readRecords acc newHeader ""
        | line ->
            let line' = line.Trim()
            readRecords acc currentHeader (currentSeq + line')
    let res = readRecords [] "" ""
    reader.Dispose()
    res

let private readOfBytes (data:byte []) =
    use ms = new System.IO.MemoryStream(data)
    use reader = new System.IO.StreamReader(ms)
    let res = read reader
    ms.Dispose()
    res

let private readOfString (str:string) =
    use reader = new System.IO.StringReader(str)
    let res = read reader
    res

let mainReadOfString(dataRaw: string) : Async<Result<DataInputItem [], exn>>=
    async {
        let data : DataInputItem [] = readOfString dataRaw |> Array.ofList
        return Ok data
    }

let mainReadOfBytes(dataRaw: byte []) : Async<Result<DataInputItem [], exn>>=
    async {
        let data : DataInputItem [] = readOfBytes dataRaw |> Array.ofList
        return Ok data
    }