$ErrorActionPreference = 'Continue'

Write-Host "`n=== Iniciando OpenLicense ===" -ForegroundColor Cyan
Write-Host "Backend:  dotnet run (Backend/)" -ForegroundColor Yellow
Write-Host "Frontend: npm run dev (Frontend/)`n" -ForegroundColor Yellow

$backendProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory ".\Backend" -PassThru -NoNewWindow
$frontendProcess = Start-Process -FilePath "npm.cmd" -ArgumentList "run", "dev" -WorkingDirectory ".\Frontend" -PassThru -NoNewWindow

Write-Host "Processos iniciados:" -ForegroundColor Green
Write-Host "  Backend  PID: $($backendProcess.Id)" -ForegroundColor Green
Write-Host "  Frontend PID: $($frontendProcess.Id)`n" -ForegroundColor Green
Write-Host "Pressione Ctrl+C para parar ambos os processos.`n" -ForegroundColor Gray

try {
    while ($true) {
        Start-Sleep -Seconds 1
        if ($backendProcess.HasExited -and $frontendProcess.HasExited) {
            Write-Host "`nAmbos os processos foram encerrados." -ForegroundColor Yellow
            break
        }
        if ($backendProcess.HasExited) {
            Write-Host "`nBackend encerrado. Frontend ainda rodando." -ForegroundColor Yellow
        }
        if ($frontendProcess.HasExited) {
            Write-Host "`nFrontend encerrado. Backend ainda rodando." -ForegroundColor Yellow
        }
    }
}
catch [System.Management.Automation.RuntimeException] {
    Write-Host "`nRecebido sinal de interrupcao (Ctrl+C)." -ForegroundColor Red
}
finally {
    Write-Host "`nEncerrando processos...`n" -ForegroundColor Yellow

    if (!$backendProcess.HasExited) {
        Write-Host "Parando Backend (PID: $($backendProcess.Id))..." -ForegroundColor Magenta
        Stop-Process -Id $backendProcess.Id -Force
    }
    if (!$frontendProcess.HasExited) {
        Write-Host "Parando Frontend (PID: $($frontendProcess.Id))..." -ForegroundColor Magenta
        Stop-Process -Id $frontendProcess.Id -Force
    }

    Write-Host "Todos os processos encerrados.`n" -ForegroundColor Green
}
