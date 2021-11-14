$poster = Get-Content -Path "$PSScriptRoot/computerVision.json" -Raw | ConvertFrom-Json -AsHashtable

$poster.analyzeResult.readResults.lines
| ForEach-Object { $_.text }
| Out-File "$PSScriptRoot/extract.txt"
