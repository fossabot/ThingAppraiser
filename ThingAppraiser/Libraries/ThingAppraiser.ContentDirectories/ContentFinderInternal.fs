﻿module internal ThingAppraiser.ContentDirectories.ContentFinderInternal

open System.IO
open System.Threading.Tasks
open ThingAppraiser.ContentDirectories


[<Struct>]
type internal ContentTypeInternal =
    | Movie
    | Image
    | Text

type internal ContentFinderArgumentsInternal = {
    DirectorySeq: seq<string>
    FileSeqGen: ContentModels.FileSeqGenerator
    ContentType: ContentTypeInternal
    DirectoryExceptionHandler: exn -> string -> unit
}

let private getPatterns (contentType: ContentTypeInternal) =
    match contentType with
        | Movie -> [ "*.mkv"; "*.mp4"; "*.flv"; "*.avi"; "*.mov"; "*.3gp" ]
        | Image -> [ "*.png"; "*.jpg"; "*.jpeg"; "*.bmp"; "*.jpe"; "*.jfif" ]
        | Text  -> [ "*.txt"; "*.md" ]

let private convertSeqGenToAsync (fileSeqGen: ContentModels.FileSeqGenerator) =
    match fileSeqGen with
        | ContentModels.FileSeqGenerator.Sync(generatorSync = genSync) ->
            fun arg1 arg2 -> Task.FromResult (genSync arg1 arg2)
        | ContentModels.FileSeqGenerator.Async(generatorAsync = genAsync) ->
            genAsync

let internal findContentAsync (args: ContentFinderArgumentsInternal) =
    async {
        let patterns = getPatterns args.ContentType

        let (innerArgs: ContentModels.ScannerArguments) = {
            FileNamePatterns = patterns
            DirectoryExceptionHandler = args.DirectoryExceptionHandler
        }

        let fileSeqGenAsync = convertSeqGenToAsync args.FileSeqGen

        let seqResults = args.DirectorySeq
                         |> Seq.filter (isNull >> not)
                         |> Seq.map (fun directoryName -> fileSeqGenAsync directoryName innerArgs)

        let! dirResults = Task.WhenAll(seqResults) |> Async.AwaitTask
        return dirResults
               |> Seq.collect (fun resultForOneDir -> resultForOneDir)
               |> Seq.groupBy (Path.GetDirectoryName >> Path.GetFileName)
    } |> Async.StartAsTask