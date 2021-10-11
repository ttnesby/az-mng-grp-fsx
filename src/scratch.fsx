#load @"./../src/mngGrpYaml.fsx"

open ManagementGroup

let x  = $"{__SOURCE_DIRECTORY__}/../src/yaml/mngGrp.yaml" |> Yaml.getYamlFile