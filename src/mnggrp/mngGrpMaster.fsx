#load @"./mngGrpTypes.fsx"

namespace ManagementGroup

module Master =

    [<RequireQualifiedAccess>]
    type MGHier =
      | Node of MG * MGHier seq
      | Leaf of MG * MGHier
      | End

    type MGHier with
        static member Hierarchy () =
            Node (MG.Top, [
                Node (MG.Landingzones, [
                    Node (MG.Online, [
                        Leaf (MG.OnlineDev, End)
                        Leaf (MG.OnlineTest, End)
                        Leaf (MG.OnlineProd, End)
                    ])
                    Node (MG.Hybrid, [
                        Leaf (MG.HybridDev, End)
                        Leaf (MG.HybridTest, End)
                        Leaf (MG.HybridProd, End)
                    ])
                ])
                Node (MG.Platform, [
                        Leaf (MG.Identity, End)
                        Leaf (MG.Connectivity, End)
                        Leaf (MG.Management, End)
                ])
                Leaf (MG.Sandbox, End)
                Leaf (MG.Decommissioned, End)
            ])

    type MGInfo = {Name: System.Guid; DisplayName: string option}

    let groups () =
        [
            (MG.Top,
                {Name = System.Guid "e925b5fc-3053-449a-9376-9e51d7435d2c"; DisplayName = Some "aCompany"})
            (MG.Landingzones,
                {Name = System.Guid "9e147ac5-eb41-4d8e-aa96-f9244b555fe8"; DisplayName = None})
            (MG.Hybrid,
                {Name = System.Guid "2d2ac951-ea76-4a7f-a86f-0c2b4517c13b"; DisplayName = None})
            (MG.HybridDev,
                {Name = System.Guid "0f7d8145-bc86-4f08-9c9b-0d5ca4e09162"; DisplayName = Some "hybrid-dev"})
            (MG.HybridTest,
                {Name = System.Guid "7064d5d9-1e41-4864-826c-85c8fbb08d51"; DisplayName = Some "hybrid-test"})
            (MG.HybridProd,
                {Name = System.Guid "4ed8e686-f62d-4cf5-865b-0241f5723a8e"; DisplayName = Some "hybrid-prod"})
            (MG.Online,
                {Name = System.Guid "9d50433e-a8fb-43fd-8712-9440b10c14a0"; DisplayName = None})
            (MG.OnlineDev,
                {Name = System.Guid "8dec9d92-d500-4c5a-82bd-5f21dd2bc5ce"; DisplayName = Some "online-dev"})
            (MG.OnlineTest,
                {Name = System.Guid "6bdd486e-80d1-4939-8988-9d045868aaee"; DisplayName = Some "online-test"})
            (MG.OnlineProd,
                {Name = System.Guid "66544828-f54a-47ff-a063-0399bfc006cd"; DisplayName = Some "online-prod"})
            (MG.Platform,
                {Name = System.Guid "47478be0-1ac5-4fb3-bf6b-5d3ac5cd65da"; DisplayName = None})
            (MG.Identity,
                {Name = System.Guid "03c346a4-7877-4033-82d0-91370b589ed1"; DisplayName = None})
            (MG.Connectivity,
                {Name = System.Guid "4c5b973e-a383-44d5-8ab7-d23b55867167"; DisplayName = None})
            (MG.Management,
                {Name = System.Guid "44e7f1f5-0950-41bf-991d-7c0043ec3e26"; DisplayName = None})
            (MG.Sandbox,
                {Name = System.Guid "d3a31dc2-4e06-4187-b71d-79ae5acfab78"; DisplayName = None})
            (MG.Decommissioned,
                {Name = System.Guid "9d2c2ec5-8d32-49ea-ad41-36f4832cf287"; DisplayName = None})
        ] |> Map.ofList