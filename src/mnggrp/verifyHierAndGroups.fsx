#load @"./mngGrp.fsx"
#load @"./../utils/aLogger.fsx"

open ManagementGroup
open ALogger

let addAndLog log rest = log $"Verify hierarchy and groups: %s{rest}"
let info = addAndLog ALog.inf

info "start"
Hierarchy.isOk ()
|> Result.bind (fun _ -> Groups.areOk ())
|> Result.mapError (fun e -> ALog.err $"{e}")
|> function | Ok _ -> 0 | Error _ -> 1
|> fun ec -> info $"finished - exit code [{ec}]"; ec
|> exit
