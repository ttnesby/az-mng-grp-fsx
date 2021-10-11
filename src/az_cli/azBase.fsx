#load @"./../utils/process.fsx"

namespace AzCli

[<RequireQualifiedAccess>]
module AzCli =

    open ALogger

    let withLogging =
        Proc.Arguments
        >> ("az" |> Proc.Name |> Proc.Public.invoke (Proc.TimeoutInSec.create 60) {ALog.getLogs() with Dbg = ignore})
        >> Result.mapError (fun e -> e.Err)
        >> ALog.logPassThroughResult (ALog.getLogs()) ("az ok","az failure")

    //az login --service-principal -u <app-id> -p <password-or-cert> --tenant <tenant>

    let private loginP (tID: System.Guid, cID: System.Guid, cS: string) =
        $"--tenant {tID.ToString()} -u {cID.ToString()} -p {cS}"

    let private loginL () = "login --service-principal" |> ALog.logPassThroughStr ALog.dbg     
    let private loginC p = $"{loginL()} {p}"

    let login = loginP >> loginC >> withLogging

    let logout () = "logout" |> ALog.logPassThroughStr ALog.dbg |> withLogging 
