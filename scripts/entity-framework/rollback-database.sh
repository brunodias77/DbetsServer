#!/bin/bash

# Script para fazer rollback do banco de dados para uma migration específica
# Uso: ./rollback-database.sh <migration_name>

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INFRA_PROJECT="$PROJECT_ROOT/src/Dbets.Infrastructure"
API_PROJECT="$PROJECT_ROOT/src/Dbets.Api"
TARGET_MIGRATION="$1"

echo "⏪ Script para rollback do banco de dados"
echo "📁 Projeto Infrastructure: $INFRA_PROJECT"
echo "📁 Projeto API: $API_PROJECT"
echo ""

# Verifica se o nome da migration foi fornecido
if [ -z "$TARGET_MIGRATION" ]; then
    echo "❌ Erro: Nome da migration de destino é obrigatório"
    echo "Uso: $0 <migration_name>"
    echo "Exemplo: $0 InitialCreate"
    echo "Para voltar ao estado inicial: $0 0"
    echo ""
    echo "📋 Migrations disponíveis:"
    cd "$PROJECT_ROOT"
    dotnet ef migrations list \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT"
    exit 1
fi

# Navega para o diretório raiz do projeto
cd "$PROJECT_ROOT"

# Lista as migrations atuais
echo "📋 Status atual das migrations:"
dotnet ef migrations list \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT"

echo ""
echo "⚠️  ATENÇÃO: Esta operação irá fazer rollback do banco de dados!"
echo "🎯 Migration de destino: $TARGET_MIGRATION"
echo "📝 Isso irá:"
echo "   - Reverter todas as alterações no banco após a migration especificada"
echo "   - DELETAR dados se houver incompatibilidade de schema"
echo "   - Manter os arquivos de migration (apenas altera o banco)"
echo ""
read -p "🤔 Deseja continuar? (s/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Ss]$ ]]; then
    echo "❌ Operação cancelada pelo usuário"
    exit 0
fi

echo ""
echo "⏳ Executando rollback para: $TARGET_MIGRATION"

# Faz o rollback
dotnet ef database update "$TARGET_MIGRATION" \
    --project "$INFRA_PROJECT" \
    --startup-project "$API_PROJECT" \
    --verbose

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Rollback executado com sucesso!"
    echo ""
    echo "📋 Status final das migrations:"
    dotnet ef migrations list \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT"
    echo ""
    echo "💡 Próximos passos:"
    echo "   - Verificar se o banco está no estado esperado"
    echo "   - Remover migrations desnecessárias: ./remove-migration.sh"
    echo "   - Criar nova migration se necessário: ./create-migration.sh"
else
    echo ""
    echo "❌ Erro ao executar rollback"
    echo "💡 Possíveis causas:"
    echo "   - Migration de destino não existe"
    echo "   - Problemas de conexão com o banco"
    echo "   - Conflitos de dados no banco"
    exit 1
fi