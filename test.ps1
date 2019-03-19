Import-Module UnitySetup -MinimumVersion 5.0.102 -ErrorAction Stop
Import-Module UnityTest -MinimumVersion 1.0.8066 -ErrorAction Stop

# Run unit tests for all unity projects
Get-UnityProjectInstance -Recurse | ForEach-Object{

    $args = @{
        'Project' = $_
        'LogFile' = (Join-Path $_.Path "RunEditorTests.log")
    }

    Write-Verbose "Running Tests on project at $($_.Path)"
    Start-UnityEditor -RunEditorTests -Batchmode -Wait @args
}