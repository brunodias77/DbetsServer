#!/bin/bash

# Script para verificar o status dos containers do Dbets

echo "📊 Status dos containers do Dbets"
echo "================================="

# Verificar se o Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando."
    exit 1
fi

# Navegar para o diretório raiz do projeto
cd "$(dirname "$0")/.."

# Mostrar status dos containers
echo "📦 Containers do projeto:"
docker-compose ps

echo ""
echo "🔗 Serviços (se rodando):"
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

# Verificar se os containers estão saudáveis
echo ""
echo "🏥 Health Check:"
if docker-compose ps | grep -q "Up"; then
    echo "✅ Pelo menos um container está rodando"
    
    # Testar conexão com PostgreSQL
    if docker-compose exec -T postgres pg_isready -U dbets_user -d dbets > /dev/null 2>&1; then
        echo "✅ PostgreSQL está respondendo"
    else
        echo "⚠️  PostgreSQL pode não estar pronto ainda"
    fi
else
    echo "❌ Nenhum container está rodando"
fi