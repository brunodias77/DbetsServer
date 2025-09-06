#!/bin/bash

# Script para aplicar migrations ao banco de dados
# Uso: ./apply-migration.sh [migration_name]

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INFRA_PROJECT="$PROJECT_ROOT/src/Dbets.Infrastructure"
API_PROJECT="$PROJECT_ROOT/src/Dbets.Api"
TARGET_MIGRATION="$1"

echo "🚀 Script para aplicar migrations"
echo "📁 Projeto Infrastructure: $INFRA_PROJECT"
echo "📁 Projeto API: $API_PROJECT"
echo ""

# Navega para o diretório raiz do projeto
cd "$PROJECT_ROOT"

# Lista as migrations atuais
echo "📋 Status atual das migrations:"
dotnet ef migrations list \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT"

echo ""

if [ -n "$TARGET_MIGRATION" ]; then
    echo "🎯 Aplicando migration específica: $TARGET_MIGRATION"
else
    echo "🎯 Aplicando todas as migrations pendentes"
fi

echo "⏳ Executando dotnet ef database update..."

# Aplica as migrations
if [ -n "$TARGET_MIGRATION" ]; then
    dotnet ef database update "$TARGET_MIGRATION" \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT" \
        --verbose
else
    dotnet ef database update \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT" \
        --verbose
fi

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Migrations aplicadas com sucesso!"
    echo ""
    echo "📋 Status final das migrations:"
    dotnet ef migrations list \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT"
    echo ""
    echo "💡 Banco de dados atualizado e pronto para uso!"
else
    echo ""
    echo "❌ Erro ao aplicar migrations"
    echo "💡 Possíveis causas:"
    echo "   - Problemas de conexão com o banco"
    echo "   - Conflitos no schema do banco"
    echo "   - Migration inválida"
    echo "   - Configuração incorreta da connection string"
    exit 1
fi