#load @"./azBase.fsx"

namespace AzCli

[<RequireQualifiedAccess>]
module AzCliMngGrp =

    open ALogger

    let private grp p = $"account management-group {p}" |> ALog.logPassThroughStr ALog.dbg
    let private invoke = grp >> AzCli.withLogging

    //az account management-group create --name [--display-name] [--parent]

    let private createP (id:System.Guid, dname:string, pid: System.Guid) =
        $"--name {id.ToString()} --display-name {dname} --parent {pid.ToString()}"

    let private createC p = $"create {p}"

    let create p = async { return ( (createP >> createC >> invoke) p) }

    //az account management-group show --name [--expand] [--query-examples] [--recurse]

    let private showP (id:System.Guid, e:bool, r:bool) =
        let fSwitch s f = if f then s else ""
        let fe = fSwitch " --expand"
        let fr = fSwitch " --recurse"

        $"--name {id.ToString()}{fe e}{fr r}"

    let private showC p = $"show {p}"

    let show = showP >> showC >> invoke