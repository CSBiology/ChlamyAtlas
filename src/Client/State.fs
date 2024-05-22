namespace App

open Shared

type Model = {
    Version: Versions
} with
    static member init() = {
            Version = Versions.init()
        }

type Msg =
    | GetVersions
    | GetVersionsResponse of Versions