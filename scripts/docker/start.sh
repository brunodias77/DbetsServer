#!/bin/bash

# Script para iniciar os containers do Dbets

echo "🚀 Iniciando containers do Dbets..."

# Verificar se o Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker primeiro."
    exit 1
fi

# Navegar para o diretório raiz do projeto
cd "$(dirname "$0")/.."

# Iniciar os containers
echo "📦 Iniciando PostgreSQL e PgAdmin..."
docker-compose up -d

if [ $? -eq 0 ]; then
    echo "✅ Containers iniciados com sucesso!"
    echo ""
    echo "📊 Status dos containers:"
    docker-compose ps
    echo ""
    echo "🔗 Serviços disponíveis:"
    echo "   PostgreSQL: localhost:5432"
    echo "   PgAdmin: http://localhost:8080"
    echo ""
    echo "📋 Credenciais:"
    echo "   Database: dbets"
    echo "   Username: dbets_user"
    echo "   Password: dbets_password"
    echo ""
    echo "   PgAdmin Email: admin@dbets.com"
    echo "   PgAdmin Password: admin123"
else
    echo "❌ Erro ao iniciar os containers."
    exit 1
fi