#load @"./mngGrpTypes.fsx"
#load @"./mngGrpMaster.fsx"
#load @"./../utils/duHelpers.fsx"

namespace ManagementGroup

[<RequireQualifiedAccess>]
module Groups =

    open Master

    let areOk : GroupsAreOk = fun () ->
        try
            let gSet = groups () |> fun m -> seq { for KeyValue(k,_) in m do yield k } |> Set.ofSeq
            let mgSet = DU.Utils.asArray<MG>() |> Set.ofArray

            Set.isSuperset gSet mgSet
            |> function
            | true -> Ok ()
            | false -> $"Missing MG type(s) in groups - {Set.difference mgSet gSet} " |> Error
        with | e ->  $"{e.Message}" |> Error

    let get : GroupsGet = fun () ->
        let dname (mg,n) = match n with | Some n -> n | None -> (DU.Utils.toString<MG> mg).ToLower()
        let toInfo (mg:MG, i:MGInfo) =
            { Info.Type = mg; Name = i.Name; DisplayName = dname (mg, i.DisplayName) }

        areOk ()
        |> Result.map (fun _ ->
                groups () |> fun m -> seq { for KeyValue(k,v) in m do yield (k,v) } |> Set.ofSeq
                |> Seq.map (fun (mg,i) -> (mg, toInfo (mg, i))) |> Map.ofSeq
            )

[<RequireQualifiedAccess>]
module Hierarchy =

    open Master

    // "approaching" catamorphism, e.g. https://fsharpforfunandprofit.com/posts/recursive-types-and-folds-1b/
    let rec private cata  fEnd fLeaf fNode h : 'r =
        let next = cata fEnd fLeaf fNode

        match h with
        | MGHier.Node (mg, hs) -> fNode mg (Seq.map next hs)
        | MGHier.Leaf (mg, h) -> fLeaf mg (next h)
        | MGHier.End -> fEnd ()

    let private toSet h : Set<MG> =

        let fEnd () = Set.empty
        let fLeaf mg _ = Set.singleton mg
        let fNode mg hs = (Set.ofSeq [mg]) + (Seq.fold (+) Set.empty hs)

        cata fEnd fLeaf fNode (h ())

    let private leaves h : Set<MG> =

        let fEnd () = Set.empty
        let fLeaf mg _ = Set.singleton mg
        let fNode _ hs = Seq.fold (+) Set.empty hs

        cata fEnd fLeaf fNode (h ())

    let private parents h : Set<MG> =

        let fEnd () = Set.empty
        let fLeaf _ _ = Set.empty
        let fNode mg hs = (Set.singleton mg) + (Seq.fold (+) Set.empty hs)

        cata fEnd fLeaf fNode (h ())

    let private parentChild h =

        let fEnd () = (None, Map.empty<MG, MG list>)
        let fLeaf mg _ = (Some mg, Map.empty)
        let fNode mg hs =
            let c = hs |> Seq.map fst |> Seq.choose id |> Seq.toList

            hs |> Seq.map snd
            |> Seq.fold (fun acc m -> acc @ (Map.toList m)) List.empty
            |> fun ml -> [(mg, c)] @ ml
            |> fun ml -> (Some mg, Map.ofList ml)

        cata fEnd fLeaf fNode (h ()) |> snd

    let isOk : HierarchyIsOk = fun () ->
        match MGHier.Hierarchy() with
        | MGHier.Node (mg, _) when mg = MG.Top ->
            let hierSet = MGHier.Hierarchy |> toSet
            let mgSet = DU.Utils.asArray<MG>() |> Set.ofArray

            Set.isSuperset hierSet mgSet
            |> function
            | true -> Ok ()
            | false -> $"Missing MG types in hierarchy - {Set.difference mgSet hierSet} " |> Error
        | _ -> "MG.Top isn't the top node" |> Error

    let asSequence : HierarchyAsSequence = fun () ->

        let doSeq _ =
            let parents = parents MGHier.Hierarchy

            let sfolder (ap: Set<Set<MG*MG>>, ac: Set<MG*MG>) (p: MG, cl: MG list) =

                let lfolder (ap: Set<MG*MG>, ac: Set<MG*MG>) (c: MG) =
                    if (Seq.contains c parents) then (ap.Add (c,p), ac) else (ap, ac.Add (c,p))

                cl |>List.fold lfolder (Set.empty<MG*MG>, ac) |> fun (api, aci) -> (ap.Add(api), aci)

            parentChild MGHier.Hierarchy |> Map.toSeq
            |> Seq.fold sfolder (Set.empty<Set<MG*MG>>, Set.empty<MG*MG>)
            |> fun (ps, cs) -> ps |> Set.toList |> List.map Set.toSeq  |> fun pl -> pl @ [(Set.toSeq cs)]
            |> List.toSeq |> Seq.filter (Seq.isEmpty >> not)

        isOk () |> Result.map doSeq

    let toAPI : HierarchyToAPI = fun topParentName ->

        let errMsg r = $"Cannot build management group api list - {r}"

        Groups.get ()
        |> function
        | Error e -> e |> errMsg |> Error
        | Ok groups ->

            let prependTop ll = groups.[MG.Top] |> fun t -> [(t.Name, t.DisplayName, topParentName)]::ll

            let mg2tuple (c, p) =
                (groups.[c],groups.[p])
                |> fun (ci,pi) -> (ci.Name, ci.DisplayName, pi.Name)

            asSequence ()
            |> Result.map (fun ss ->
                ss |> Seq.map (fun s ->
                    s |> Seq.map mg2tuple |> Seq.toList) |> Seq.toList
                )
            |> Result.map prependTop