#!/usr/bin/env bash

# Cores
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
RED='\033[0;31m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m'

# --- Pré-validações ---
for dir in Backend Frontend; do
    if [ ! -d "$dir" ]; then
        echo -e "${RED}Erro: diretório '${dir}/' não encontrado na raiz.${NC}"
        exit 1
    fi
done

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Erro: 'dotnet' não está instalado.${NC}"
    exit 1
fi

if ! command -v npm &> /dev/null; then
    echo -e "${RED}Erro: 'npm' não está instalado.${NC}"
    exit 1
fi

echo -e "\n${CYAN}=== Iniciando OpenLicense ===${NC}"
echo -e "${YELLOW}Backend:  dotnet run (Backend/)${NC}"
echo -e "${YELLOW}Frontend: npm run dev (Frontend/)\n${NC}"

# PIDs monitorados
pids=()

cleanup() {
    echo -e "\n${YELLOW}Encerrando processos...${NC}"

    for pid in "${pids[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            echo -e "${MAGENTA}Parando PID: $pid${NC}"
            kill "$pid" 2>/dev/null || true
        fi
    done

    for pid in "${pids[@]}"; do
        wait "$pid" 2>/dev/null || true
    done

    echo -e "${GREEN}Todos os processos encerrados.${NC}\n"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Inicia backend
(
    cd Backend
    dotnet run
) &
pids+=($!)

# Inicia frontend
(
    cd Frontend
    npm run dev
) &
pids+=($!)

echo -e "${GREEN}Processos iniciados:${NC}"
for i in "${!pids[@]}"; do
    labels=("Backend" "Frontend")
    echo -e "  ${labels[$i]}  PID: ${pids[$i]}"
done
echo ""
echo -e "${GRAY}Pressione Ctrl+C para parar ambos os processos.${NC}"

# Loop principal
while true; do
    sleep 1
    alive=0

    for i in "${!pids[@]}"; do
        labels=("Backend" "Frontend")
        if kill -0 "${pids[$i]}" 2>/dev/null; then
            ((alive++))
        else
            echo -e "\n${YELLOW}${labels[$i]} (PID: ${pids[$i]}) encerrado inesperadamente.${NC}"
            unset 'pids[$i]'
            pids=("${pids[@]}")
        fi
    done

    if [ "$alive" -eq 0 ]; then
        echo -e "\n${YELLOW}Todos os processos foram encerrados.${NC}"
        break
    fi
done

cleanup
