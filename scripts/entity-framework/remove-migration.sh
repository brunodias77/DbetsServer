#!/bin/bash

# Script para remover a última migration do Entity Framework
# Uso: ./remove-migration.sh [--force]

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INFRA_PROJECT="$PROJECT_ROOT/src/Dbets.Infrastructure"
API_PROJECT="$PROJECT_ROOT/src/Dbets.Api"
FORCE_FLAG="$1"

echo "🗑️  Script para remover migration"
echo "📁 Projeto Infrastructure: $INFRA_PROJECT"
echo "📁 Projeto API: $API_PROJECT"
echo ""

# Navega para o diretório raiz do projeto
cd "$PROJECT_ROOT"

# Lista as migrations atuais
echo "📋 Listando migrations atuais..."
dotnet ef migrations list \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT"

echo ""

# Verifica se deve prosseguir sem confirmação
if [ "$FORCE_FLAG" != "--force" ]; then
    echo "⚠️  ATENÇÃO: Esta operação irá remover a ÚLTIMA migration criada!"
    echo "📝 Isso irá:"
    echo "   - Deletar os arquivos da migration"
    echo "   - Reverter o modelo do DbContext"
    echo "   - NÃO irá alterar o banco de dados"
    echo ""
    read -p "🤔 Deseja continuar? (s/N): " -n 1 -r
    echo ""
    
    if [[ ! $REPLY =~ ^[Ss]$ ]]; then
        echo "❌ Operação cancelada pelo usuário"
        exit 0
    fi
fi

echo ""
echo "⏳ Removendo a última migration..."

# Remove a migration
dotnet ef migrations remove \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT" \
    --verbose

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Migration removida com sucesso!"
    echo ""
    echo "📋 Migrations restantes:"
    dotnet ef migrations list \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT"
    echo ""
    echo "💡 Dicas:"
    echo "   - Se o banco já foi atualizado, use: ./rollback-database.sh"
    echo "   - Para criar uma nova migration: ./create-migration.sh NomeDaMigration"
else
    echo ""
    echo "❌ Erro ao remover a migration"
    echo "💡 Possíveis causas:"
    echo "   - Não há migrations para remover"
    echo "   - Migration já foi aplicada ao banco (use rollback primeiro)"
    echo "   - Problemas de configuração do projeto"
    exit 1
fi