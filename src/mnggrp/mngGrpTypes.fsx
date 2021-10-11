// Top must exist, define anything else
// See companion mngGrp.yaml for hierarchy and groups

namespace ManagementGroup

[<RequireQualifiedAccess>]
type MG = 
    | Top // mandatory top node, all others are based on requirements
    | Landingzones
    | Hybrid
    | HybridDev
    | HybridTest
    | HybridProd
    | Online
    | OnlineDev
    | OnlineTest
    | OnlineProd
    | Platform
    | Connectivity
    | Identity
    | Management
    | Sandbox
    | Decommissioned

type Info = {
    Type: MG
    Name: System.Guid 
    DisplayName: string
}

type GroupsAreOk = unit -> Result<unit,string>
type GroupsGet = unit -> Result<Map<MG,Info>,string>

type HierarchyIsOk = unit -> Result<unit, string>
type HierarchyAsSequence = unit -> Result<seq<seq<MG*MG>>, string>

// child name * child display name * parent name
type HierarchyToAPI = System.Guid -> Result<list<list<System.Guid*string*System.Guid>>,string>