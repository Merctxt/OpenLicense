#!/usr/bin/env bash

set -e

# Cores
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
RED='\033[0;31m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m'

echo -e "\n${CYAN}=== Iniciando OpenLicense ===${NC}"
echo -e "${YELLOW}Backend:  dotnet run (Backend/)${NC}"
echo -e "${YELLOW}Frontend: npm run dev (Frontend/)\n${NC}"

# Inicia backend
(
    cd Backend
    dotnet run
) &
BACKEND_PID=$!

# Inicia frontend
(
    cd Frontend
    npm run dev
) &
FRONTEND_PID=$!

echo -e "${GREEN}Processos iniciados:${NC}"
echo -e "  Backend  PID: ${BACKEND_PID}"
echo -e "  Frontend PID: ${FRONTEND_PID}\n"
echo -e "${GRAY}Pressione Ctrl+C para parar ambos os processos.${NC}"

cleanup() {
    echo -e "\n${YELLOW}Encerrando processos...${NC}"

    if kill -0 "$BACKEND_PID" 2>/dev/null; then
        echo -e "${MAGENTA}Parando Backend (PID: $BACKEND_PID)...${NC}"
        kill "$BACKEND_PID" 2>/dev/null || true
    fi

    if kill -0 "$FRONTEND_PID" 2>/dev/null; then
        echo -e "${MAGENTA}Parando Frontend (PID: $FRONTEND_PID)...${NC}"
        kill "$FRONTEND_PID" 2>/dev/null || true
    fi

    # Aguarda processo finalizarem
    wait "$BACKEND_PID" 2>/dev/null || true
    wait "$FRONTEND_PID" 2>/dev/null || true

    echo -e "${GREEN}Todos os processos encerrados.${NC}\n"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Aguarda processos
while kill -0 "$BACKEND_PID" 2>/dev/null || kill -0 "$FRONTEND_PID" 2>/dev/null; do
    sleep 1

    if ! kill -0 "$BACKEND_PID" 2>/dev/null && kill -0 "$FRONTEND_PID" 2>/dev/null; then
        echo -e "\n${YELLOW}Backend encerrado. Frontend ainda rodando.${NC}"
    fi

    if kill -0 "$BACKEND_PID" 2>/dev/null && ! kill -0 "$FRONTEND_PID" 2>/dev/null; then
        echo -e "\n${YELLOW}Frontend encerrado. Backend ainda rodando.${NC}"
    fi

    if ! kill -0 "$BACKEND_PID" 2>/dev/null && ! kill -0 "$FRONTEND_PID" 2>/dev/null; then
        echo -e "\n${YELLOW}Ambos os processos foram encerrados.${NC}"
        break
    fi
done

cleanup
