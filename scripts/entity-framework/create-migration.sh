#!/bin/bash

# Script para criar uma nova migration no Entity Framework
# Uso: ./create-migration.sh NomeDaMigration

set -e

# Verifica se o nome da migration foi fornecido
if [ -z "$1" ]; then
    echo "❌ Erro: Nome da migration é obrigatório"
    echo "Uso: $0 <NomeDaMigration>"
    echo "Exemplo: $0 AddUserTable"
    exit 1
fi

MIGRATION_NAME="$1"
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INFRA_PROJECT="$PROJECT_ROOT/src/Dbets.Infrastructure"
API_PROJECT="$PROJECT_ROOT/src/Dbets.Api"

echo "🚀 Criando migration: $MIGRATION_NAME"
echo "📁 Projeto Infrastructure: $INFRA_PROJECT"
echo "📁 Projeto API: $API_PROJECT"
echo ""

# Navega para o diretório raiz do projeto
cd "$PROJECT_ROOT"

# Cria a migration
echo "⏳ Executando dotnet ef migrations add..."
dotnet ef migrations add "$MIGRATION_NAME" \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT" \
    --verbose

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Migration '$MIGRATION_NAME' criada com sucesso!"
    echo "📂 Verifique os arquivos gerados em: src/Dbets.Infrastructure/Migrations/"
    echo ""
    echo "💡 Próximos passos:"
    echo "   1. Revisar a migration gerada"
    echo "   2. Aplicar ao banco: ./apply-migration.sh"
    echo "   3. Ou reverter se necessário: ./remove-migration.sh"
else
    echo ""
    echo "❌ Erro ao criar a migration '$MIGRATION_NAME'"
    exit 1
fi