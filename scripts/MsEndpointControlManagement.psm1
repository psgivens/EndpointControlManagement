#!/usr/bin/pwsh

Function Test-PomMissing {
    if (-not $env:POMODORO_REPOS) {
        Write-Host "Please set the $env:POMODORO_REPOS to the location of this repo."
        return $true
    }   
}

Function Use-PomDirectory {
    if (Test-PomMissing) { RETURN }
    Set-Location "$env:POMODORO_REPOS/PersonalTracker.Api"
}

Function Start-PmsEndpointControlManagement {
    param(
        # [Parameter(
        #     Mandatory=$true, 
        #     HelpMessage="Starts IdentiyManagement microservices.",
        #     ParameterSetName="Individual")]
        # [ValidateSet(
        #     "pomodoro-pgsql",
        #     "pomodoro-idserver",
        #     "pomodoro-identity", 
        #     "pomodoro-resource", 
        #     "pomodoro-privilege", 
        #     "pomodoro-reverse-proxy",
        #     "watch-pomo-rapi",
        #     "pomo-ping-rapi",
        #     "pomodoro-client"
        # )] 
        # [string]$Container,
        [Parameter(
            Mandatory=$false, 
            HelpMessage="Use 'dotnet run'")]
        [switch]$Runs
    )

    if (Test-PomMissing) { RETURN }

    Write-Host "Starting pms-endpointcontrolmanagement..."

    if ($Runs) {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
            --name pms-endpointcontrolmanagement `
            --rm -it `
            -p 8080:8080 `
            --network pomodoro-net `
            --entrypoint "/bin/bash" `
            -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement/src/:/app/EndpointControlManagement/EndpointControlManagement/src/ `
            -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement/secrets/:/app/EndpointControlManagement/EndpointControlManagement/secrets/ `
            -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.Domain/src/:/app/EndpointControlManagement/EndpointControlManagement.Domain/src/ `
            -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.Domain.DAL/src/:/app/EndpointControlManagement/EndpointControlManagement.Domain.DAL/src/ `
            -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.UnitTests/src/:/app/EndpointControlManagement/EndpointControlManagement.UnitTests/src/ `
            pms-endpointcontrolmanagement
#            pms-endpointcontrolmanagement "run" "--project" "EndpointControlManagement"
    } else {
        # Cannot attach a debugger, but can have the app auto reload during development.
        # https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md
        docker run `
        --name pms-endpointcontrolmanagement `
        --rm -it `
        -p 2005:8080 `
        --network pomodoro-net `
        -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement/src/:/app/EndpointControlManagement/EndpointControlManagement/src/ `
        -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement/secrets/:/app/EndpointControlManagement/EndpointControlManagement/secrets/ `
        -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.Domain/src/:/app/EndpointControlManagement/EndpointControlManagement.Domain/src/ `
        -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.Domain.DAL/src/:/app/EndpointControlManagement/EndpointControlManagement.Domain.DAL/src/ `
        -v $env:POMODORO_REPOS/EndpointControlManagement/EndpointControlManagement.UnitTests/src/:/app/EndpointControlManagement/EndpointControlManagement.UnitTests/src/ `
        pms-endpointcontrolmanagement

    }
}
Function Build-PmsEndpointControlManagement {
    <#
    .SYNOPSIS
        Builds the docker container related to the pomodor project.
    .DESCRIPTION
        Builds the docker container related to the pomodor project.
    .PARAMETER Image
        One of the valid images for the pomodoro project
    .EXAMPLE
    .NOTES
        Author: Phillip Scott Givens
    #>    
    param(
        [Parameter(Mandatory=$false)]
        [ValidateSet(
            "docker", 
            "microk8s.docker",
            "azure"
            )] 
        [string]$Docker="docker"
    )

    if (Test-PomMissing) { RETURN }
    if ($Docker) {
        Set-Alias dkr $Docker -Option Private
    }

    $buildpath = "$env:POMODORO_REPOS/EndpointControlManagement"
    dkr build `
        -t pms-endpointcontrolmanagement `
        -f "$buildpath/watch.Dockerfile" `
        "$buildpath/.."
}

Function Update-PmsEndpointControlManagement {
    if (Test-PomMissing) { RETURN }

    $MyPSModulePath = "{0}/.local/share/powershell/Modules" -f (ls -d ~)
    mkdir -p $MyPSModulePath/MsEndpointControlManagement

    Write-Host ("Linking {0}/EndpointControlManagement/scripts/MsEndpointControlManagement.psm1 to {1}/MsEndpointControlManagement/" -f $env:POMODORO_REPOS,  $MyPSModulePath)
    ln -s $env:POMODORO_REPOS/EndpointControlManagement/scripts/MsEndpointControlManagement.psm1  $MyPSModulePath/MsEndpointControlManagement/MsEndpointControlManagement.psm1

    Write-Host "Force import-module PomodorEnv"
    Import-Module -Force MsEndpointControlManagement -Global

}



Export-ModuleMember -Function Build-PmsEndpointControlManagement
Export-ModuleMember -Function Start-PmsEndpointControlManagement
Export-ModuleMember -Function Update-PmsEndpointControlManagement