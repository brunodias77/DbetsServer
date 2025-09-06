#!/bin/bash

# Script para parar os containers do Dbets

echo "🛑 Parando containers do Dbets..."

# Verificar se o Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando."
    exit 1
fi

# Navegar para o diretório raiz do projeto
cd "$(dirname "$0")/.."

# Verificar se existem containers rodando
if [ -z "$(docker-compose ps -q)" ]; then
    echo "ℹ️  Nenhum container do projeto está rodando."
    exit 0
fi

# Parar os containers
echo "📦 Parando PostgreSQL e PgAdmin..."
docker-compose down

if [ $? -eq 0 ]; then
    echo "✅ Containers parados com sucesso!"
    echo ""
    echo "📊 Status dos containers:"
    docker-compose ps
else
    echo "❌ Erro ao parar os containers."
    exit 1
fi

# Opção para remover volumes (comentada por segurança)
# echo ""
# read -p "🗑️  Deseja remover os volumes de dados? (y/N): " -n 1 -r
# echo
# if [[ $REPLY =~ ^[Yy]$ ]]; then
#     echo "🗑️  Removendo volumes..."
#     docker-compose down -v
#     echo "✅ Volumes removidos!"
# fi