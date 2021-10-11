#load @"./mnggrp/mngGrp.fsx"
#load @"./az_cli/azAccountMngGrp.fsx"

open ALogger
open ManagementGroup
open AzCli

type Params = {
    TenantID    : System.Guid
    ClientID    : System.Guid
    Secret      : string
    AnchorName  : System.Guid
}
module Helpers =

    let addAndLog log rest = log $"Deploy management group: %s{rest}"
    let info = addAndLog ALog.inf

    let getParams () =
        let errMsg e = $"Missing tenantID, clientID, secret, anchorID - or invalid GUID - {e}"
        try {
                TenantID    = fsi.CommandLineArgs.[1] |> System.Guid.Parse
                ClientID    = fsi.CommandLineArgs.[2] |> System.Guid.Parse
                Secret      = fsi.CommandLineArgs.[3]
                AnchorName  = fsi.CommandLineArgs.[4] |> System.Guid.Parse
            } |> Ok
        with | e -> e.Message |> errMsg |> Error

    let deployWorkList (ll: (System.Guid*string*System.Guid) list list) =

        let errToSome (r: Result<'o,'e>) =  match r with | Error _ -> Some true | Ok _ -> None
        let maintainMngGrp l =
            l |> List.map AzCliMngGrp.create |> Async.Parallel |> Async.RunSynchronously |> Array.toList

        ll |> List.collect maintainMngGrp |> List.map errToSome |> List.choose id |> List.isEmpty
        |> function
        | true -> Ok ()
        | false -> Error "Failure during deploy of management group work list"

    let logError r = match r with | Error e -> ALog.err $"{e}"; Error () | Ok _ -> Ok ()
    let result2ExitCode r = match r with | Ok _ -> 0 | Error _ -> 1

Helpers.info "start"
ALog.inf "Get params tenantID, clientID, secret, anchorID"
Helpers.getParams ()
|> function 
| Error e -> Error e
| Ok p ->

    ALog.inf "Build management group work list"
    Hierarchy.toAPI p.AnchorName
    |> function 
    | Error e -> Error e
    | Ok worklist ->

        AzCli.login (p.TenantID, p.ClientID, p.Secret)
        |> Result.bind (fun _ -> AzCliMngGrp.show (p.AnchorName, false, false))
        |> Result.map (ALog.logPassThroughX ALog.inf "Deploy management group work list")
        |> Result.bind (fun _ -> Helpers.deployWorkList worklist)
        |> fun r -> (AzCli.logout >> ignore) (); r

|> Helpers.logError
|> Helpers.result2ExitCode
|> fun ec -> Helpers.info $"finished - exit code [{ec}]"; ec
|> exit