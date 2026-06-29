$ContainerName = "authorization-api"

$existing = docker ps -aq --filter "name=^${ContainerName}$"
if (-not $existing) {
    Write-Host "No container named '$ContainerName' is running."
    exit 0
}

Write-Host "Stopping and removing container '$ContainerName'..."
docker rm -f $existing | Out-Null

Write-Host "Container '$ContainerName' removed."
