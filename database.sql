-- Habilita a extensão UUID caso ainda não esteja habilitada.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela de usuários
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    profile_picture VARCHAR(500),
    active BOOLEAN DEFAULT true,
    email_confirmed BOOLEAN DEFAULT false,
    login_attempts INT DEFAULT 0,
    locked_until TIMESTAMPTZ NULL,
    last_login TIMESTAMPTZ NULL,
    theme VARCHAR(10) DEFAULT 'light' CONSTRAINT CK_users_theme CHECK (theme IN ('light', 'dark')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL
);

-- Tabela de tokens de refresh
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked BOOLEAN DEFAULT false,
    revoked_at TIMESTAMPTZ NULL,
    revoked_reason VARCHAR(100) NULL,
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de confirmações de email
CREATE TABLE email_confirmations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    token UUID NOT NULL UNIQUE DEFAULT uuid_generate_v4(),
    expires_at TIMESTAMPTZ NOT NULL,
    confirmed BOOLEAN DEFAULT false,
    confirmed_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_email_confirmations_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de reset de senhas
CREATE TABLE password_resets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    token UUID NOT NULL UNIQUE DEFAULT uuid_generate_v4(),
    expires_at TIMESTAMPTZ NOT NULL,
    used BOOLEAN DEFAULT false,
    used_at TIMESTAMPTZ NULL,
    ip_address VARCHAR(45),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_password_resets_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de casas de apostas
CREATE TABLE bookmakers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    website VARCHAR(255),
    logo_url VARCHAR(500),
    commission_rate NUMERIC(5,4) DEFAULT 0.00, -- Taxa de comissão da casa
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT FK_bookmakers_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT UK_bookmakers_user_name UNIQUE (user_id, name)
);

-- Tabela de mercados de apostas
CREATE TABLE markets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255),
    category VARCHAR(50),
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL
);

-- Tabela de jogos
CREATE TABLE games (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    home_team VARCHAR(100) NOT NULL,
    away_team VARCHAR(100) NOT NULL,
    championship VARCHAR(100),
    game_date TIMESTAMPTZ NOT NULL,
    game_status VARCHAR(20) DEFAULT 'Scheduled' 
        CONSTRAINT CK_games_status CHECK (game_status IN ('Scheduled', 'Live', 'Finished', 'Postponed', 'Cancelled')),
    home_score INT NULL CONSTRAINT CK_home_score_positive CHECK (home_score >= 0),
    away_score INT NULL CONSTRAINT CK_away_score_positive CHECK (away_score >= 0),
    sport VARCHAR(50) DEFAULT 'Football', -- Adicionado campo esporte
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT FK_games_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Tabela de tipos de apostas
CREATE TABLE bet_types (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL
);

-- Tabela principal de apostas
CREATE TABLE bets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    bet_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    bookmaker_id UUID NOT NULL,
    bet_type_id UUID NOT NULL,
    stake NUMERIC(10,2) NOT NULL,
    total_odds NUMERIC(8,3) NOT NULL,
    bet_status VARCHAR(20) DEFAULT 'Pending' 
        CONSTRAINT CK_bets_status CHECK (bet_status IN ('Pending', 'Won', 'Lost', 'Void', 'Partially_Won')),
    return_value NUMERIC(10,2) NULL,
    gross_profit NUMERIC(10,2) NULL,
    net_profit NUMERIC(10,2) NULL,
    profit_units NUMERIC(8,3) NULL,
    commission NUMERIC(8,2) DEFAULT 0.00, -- Comissão paga
    notes VARCHAR(500),
    settled_at TIMESTAMPTZ NULL, -- Quando a aposta foi liquidada
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT FK_bets_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_bets_bookmaker FOREIGN KEY (bookmaker_id) REFERENCES bookmakers(id),
    CONSTRAINT FK_bets_bet_type FOREIGN KEY (bet_type_id) REFERENCES bet_types(id),
    CONSTRAINT CK_stake_positive CHECK (stake > 0),
    CONSTRAINT CK_total_odds_positive CHECK (total_odds > 0),
    CONSTRAINT CK_return_value_positive CHECK (return_value IS NULL OR return_value >= 0)
);

-- Tabela de detalhes das apostas (seleções individuais)
CREATE TABLE bet_details (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    bet_id UUID NOT NULL,
    game_id UUID NOT NULL,
    market_id UUID NOT NULL,
    selection VARCHAR(200) NOT NULL,
    odd NUMERIC(8,3) NOT NULL,
    result VARCHAR(20) 
        CONSTRAINT CK_bet_details_result CHECK (result IS NULL OR result IN ('Won', 'Lost', 'Void', 'Pushed')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT FK_details_bet FOREIGN KEY (bet_id) REFERENCES bets(id) ON DELETE CASCADE,
    CONSTRAINT FK_details_game FOREIGN KEY (game_id) REFERENCES games(id),
    CONSTRAINT FK_details_market FOREIGN KEY (market_id) REFERENCES markets(id),
    CONSTRAINT CK_odd_positive CHECK (odd > 0)
);

-- Tabela para histórico de saldo (opcional, para auditoria)
CREATE TABLE balance_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    bet_id UUID NULL,
    transaction_type VARCHAR(20) NOT NULL 
        CONSTRAINT CK_transaction_type CHECK (transaction_type IN ('Deposit', 'Withdrawal', 'Bet_Placed', 'Bet_Won', 'Bet_Lost', 'Commission')),
    amount NUMERIC(10,2) NOT NULL,
    balance_before NUMERIC(12,2) NOT NULL,
    balance_after NUMERIC(12,2) NOT NULL,
    description VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_balance_history_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_balance_history_bet FOREIGN KEY (bet_id) REFERENCES bets(id)
);

-- Índices para melhor performance
CREATE INDEX IDX_users_email ON users(email);
CREATE INDEX IDX_users_active ON users(active) WHERE active = true;
CREATE INDEX IDX_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX IDX_refresh_tokens_expires ON refresh_tokens(expires_at);
CREATE INDEX IDX_bets_user_id ON bets(user_id);
CREATE INDEX IDX_bets_date ON bets(bet_date);
CREATE INDEX IDX_bets_status ON bets(bet_status);
CREATE INDEX IDX_bet_details_bet_id ON bet_details(bet_id);
CREATE INDEX IDX_games_date ON games(game_date);
CREATE INDEX IDX_games_user_status ON games(user_id, game_status);
CREATE INDEX IDX_balance_history_user_id ON balance_history(user_id);
CREATE INDEX IDX_balance_history_date ON balance_history(created_at);

-- Triggers para updated_at (exemplo para tabela users)
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_users_updated_at 
    BEFORE UPDATE ON users 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_bets_updated_at 
    BEFORE UPDATE ON bets 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_games_updated_at 
    BEFORE UPDATE ON games 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();