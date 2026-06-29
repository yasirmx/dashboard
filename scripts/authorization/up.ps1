$ImageName  = "authorization-api"
$ContainerName = "authorization-api"
$HostPort   = 7000
$DockerfileContext = "$PSScriptRoot\..\..\src\services\authorization"

Write-Host "Building image '$ImageName'..."
docker build -t $ImageName $DockerfileContext
if ($LASTEXITCODE -ne 0) {
    Write-Error "docker build failed."
    exit $LASTEXITCODE
}

# Remove any existing container with the same name
$existing = docker ps -aq --filter "name=^${ContainerName}$"
if ($existing) {
    Write-Host "Removing existing container '$ContainerName'..."
    docker rm -f $existing | Out-Null
}

Write-Host "Starting container '$ContainerName' on port $HostPort..."
docker run -d `
    --name $ContainerName `
    -p "${HostPort}:7000" `
    $ImageName

if ($LASTEXITCODE -ne 0) {
    Write-Error "docker run failed."
    exit $LASTEXITCODE
}

Write-Host "Container '$ContainerName' is running. API available at http://localhost:$HostPort"
