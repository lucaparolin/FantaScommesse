-- =============================================
-- FantaScommesse Database Schema
-- Version: 1.0
-- Description: Complete database schema for FantaScommesse PWA
-- =============================================

-- Drop tables if exist (for clean re-run)
IF OBJECT_ID('fs_penalty_event', 'U') IS NOT NULL DROP TABLE fs_penalty_event;
IF OBJECT_ID('fs_bonus_event', 'U') IS NOT NULL DROP TABLE fs_bonus_event;
IF OBJECT_ID('fs_score', 'U') IS NOT NULL DROP TABLE fs_score;
IF OBJECT_ID('fs_prediction_item', 'U') IS NOT NULL DROP TABLE fs_prediction_item;
IF OBJECT_ID('fs_prediction', 'U') IS NOT NULL DROP TABLE fs_prediction;
IF OBJECT_ID('fs_referral', 'U') IS NOT NULL DROP TABLE fs_referral;
IF OBJECT_ID('fs_payment', 'U') IS NOT NULL DROP TABLE fs_payment;
IF OBJECT_ID('fs_prize_rule', 'U') IS NOT NULL DROP TABLE fs_prize_rule;
IF OBJECT_ID('fs_participation', 'U') IS NOT NULL DROP TABLE fs_participation;
IF OBJECT_ID('fs_match', 'U') IS NOT NULL DROP TABLE fs_match;
IF OBJECT_ID('fs_round', 'U') IS NOT NULL DROP TABLE fs_round;
IF OBJECT_ID('fs_team', 'U') IS NOT NULL DROP TABLE fs_team;
IF OBJECT_ID('fs_season', 'U') IS NOT NULL DROP TABLE fs_season;
IF OBJECT_ID('fs_user', 'U') IS NOT NULL DROP TABLE fs_user;
GO

-- =============================================
-- Table: fs_user
-- Description: Users of the system (participants, organizers, admins)
-- =============================================
CREATE TABLE fs_user (
    user_id           BIGINT IDENTITY(1,1) PRIMARY KEY,
    email             NVARCHAR(255) UNIQUE NOT NULL,
    password_hash     NVARCHAR(255) NOT NULL,
    display_name      NVARCHAR(80) NOT NULL,
    phone             NVARCHAR(32) NULL,
    is_admin          BIT NOT NULL DEFAULT 0,
    is_organizer      BIT NOT NULL DEFAULT 0,
    created_utc       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    status            TINYINT NOT NULL DEFAULT 1, -- 1 active, 0 disabled
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL
);
GO

-- =============================================
-- Table: fs_season
-- Description: Football seasons (e.g., Serie A 2025/26)
-- =============================================
CREATE TABLE fs_season (
    season_id         INT IDENTITY(1,1) PRIMARY KEY,
    name              NVARCHAR(50) NOT NULL,
    year_start        SMALLINT NOT NULL,
    year_end          SMALLINT NOT NULL,
    signup_deadline   DATETIME2 NOT NULL,
    base_fee_eur      DECIMAL(10,2) NOT NULL DEFAULT 100.00,
    referral_discount DECIMAL(10,2) NOT NULL DEFAULT 5.00,
    created_by        BIGINT NOT NULL,
    is_closed         BIT NOT NULL DEFAULT 0,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_season_created_by FOREIGN KEY (created_by) REFERENCES fs_user(user_id)
);
GO

-- =============================================
-- Table: fs_team
-- Description: Football teams (Serie A clubs)
-- =============================================
CREATE TABLE fs_team (
    team_id           INT IDENTITY(1,1) PRIMARY KEY,
    name              NVARCHAR(80) UNIQUE NOT NULL,
    logo_url          NVARCHAR(500) NULL,
    api_team_id       NVARCHAR(50) NULL, -- External API reference
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL
);
GO

-- =============================================
-- Table: fs_round
-- Description: Match rounds (giornate) within a season
-- =============================================
CREATE TABLE fs_round (
    round_id          INT IDENTITY(1,1) PRIMARY KEY,
    season_id         INT NOT NULL,
    round_no          INT NOT NULL,
    deadline_utc      DATETIME2 NOT NULL,
    is_published      BIT NOT NULL DEFAULT 0,
    is_closed         BIT NOT NULL DEFAULT 0,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_round_season FOREIGN KEY (season_id) REFERENCES fs_season(season_id),
    CONSTRAINT UQ_round_season_no UNIQUE(season_id, round_no)
);
GO

-- =============================================
-- Table: fs_match
-- Description: Individual matches within a round
-- =============================================
CREATE TABLE fs_match (
    match_id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    round_id          INT NOT NULL,
    order_no          TINYINT NOT NULL CHECK(order_no BETWEEN 1 AND 10),
    home_team_id      INT NOT NULL,
    away_team_id      INT NOT NULL,
    kickoff_utc       DATETIME2 NOT NULL,
    result_code       CHAR(1) NULL CHECK (result_code IN ('1','X','2')),
    goals_home        TINYINT NULL,
    goals_away        TINYINT NULL,
    ggng              CHAR(2) NULL CHECK (ggng IN ('GG','NG')),
    ou                CHAR(5) NULL CHECK (ou IN ('OVER','UNDER')),
    api_match_id      NVARCHAR(50) NULL, -- External API reference
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_match_round FOREIGN KEY (round_id) REFERENCES fs_round(round_id),
    CONSTRAINT FK_match_home_team FOREIGN KEY (home_team_id) REFERENCES fs_team(team_id),
    CONSTRAINT FK_match_away_team FOREIGN KEY (away_team_id) REFERENCES fs_team(team_id),
    CONSTRAINT UQ_match_round_order UNIQUE(round_id, order_no)
);
GO

-- =============================================
-- Table: fs_participation
-- Description: User participation in a season
-- =============================================
CREATE TABLE fs_participation (
    participation_id  BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id           BIGINT NOT NULL,
    season_id         INT NOT NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_participation_user FOREIGN KEY (user_id) REFERENCES fs_user(user_id),
    CONSTRAINT FK_participation_season FOREIGN KEY (season_id) REFERENCES fs_season(season_id),
    CONSTRAINT UQ_participation_user_season UNIQUE(user_id, season_id)
);
GO

-- =============================================
-- Table: fs_prediction
-- Description: User predictions for a round (colonna)
-- =============================================
CREATE TABLE fs_prediction (
    prediction_id     BIGINT IDENTITY(1,1) PRIMARY KEY,
    participation_id  BIGINT NOT NULL,
    round_id          INT NOT NULL,
    created_utc       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    submitted_utc     DATETIME2 NULL, -- NULL = draft
    is_valid          BIT NOT NULL DEFAULT 0,
    is_late           BIT NOT NULL DEFAULT 0,
    errors_cnt        TINYINT NOT NULL DEFAULT 0,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_prediction_participation FOREIGN KEY (participation_id) REFERENCES fs_participation(participation_id),
    CONSTRAINT FK_prediction_round FOREIGN KEY (round_id) REFERENCES fs_round(round_id),
    CONSTRAINT UQ_prediction_participation_round UNIQUE(participation_id, round_id)
);
GO

-- =============================================
-- Table: fs_prediction_item
-- Description: Individual match predictions within a colonna
-- =============================================
CREATE TABLE fs_prediction_item (
    item_id           BIGINT IDENTITY(1,1) PRIMARY KEY,
    prediction_id     BIGINT NOT NULL,
    match_id          BIGINT NOT NULL,
    selection         NVARCHAR(5) NOT NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_prediction_item_prediction FOREIGN KEY (prediction_id) REFERENCES fs_prediction(prediction_id),
    CONSTRAINT FK_prediction_item_match FOREIGN KEY (match_id) REFERENCES fs_match(match_id),
    CONSTRAINT CHK_prediction_item_selection CHECK (selection IN ('1','X','2','1X','X2','12','GG','NG','OVER','UNDER')),
    CONSTRAINT UQ_prediction_item_prediction_match UNIQUE(prediction_id, match_id)
);
GO

-- =============================================
-- Table: fs_score
-- Description: Calculated scores for predictions
-- =============================================
CREATE TABLE fs_score (
    score_id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    prediction_id     BIGINT NOT NULL,
    points_base       TINYINT NOT NULL DEFAULT 0, -- 0..10
    bonus             SMALLINT NOT NULL DEFAULT 0,
    penalty           SMALLINT NOT NULL DEFAULT 0,
    total             SMALLINT NOT NULL,
    computed_utc      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_score_prediction FOREIGN KEY (prediction_id) REFERENCES fs_prediction(prediction_id),
    CONSTRAINT UQ_score_prediction UNIQUE(prediction_id)
);
GO

-- =============================================
-- Table: fs_bonus_event
-- Description: Bonus events awarded to participants
-- =============================================
CREATE TABLE fs_bonus_event (
    bonus_id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    round_id          INT NOT NULL,
    participation_id  BIGINT NOT NULL,
    kind              NVARCHAR(30) NOT NULL,
    value             SMALLINT NOT NULL,
    details           NVARCHAR(255) NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_bonus_event_round FOREIGN KEY (round_id) REFERENCES fs_round(round_id),
    CONSTRAINT FK_bonus_event_participation FOREIGN KEY (participation_id) REFERENCES fs_participation(participation_id)
);
GO

-- =============================================
-- Table: fs_penalty_event
-- Description: Penalty events applied to predictions
-- =============================================
CREATE TABLE fs_penalty_event (
    penalty_id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    prediction_id     BIGINT NOT NULL,
    kind              NVARCHAR(30) NOT NULL,
    value             SMALLINT NOT NULL,
    details           NVARCHAR(255) NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_penalty_event_prediction FOREIGN KEY (prediction_id) REFERENCES fs_prediction(prediction_id)
);
GO

-- =============================================
-- Table: fs_payment
-- Description: Payment tracking for participations
-- =============================================
CREATE TABLE fs_payment (
    payment_id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    participation_id  BIGINT NOT NULL,
    expected_eur      DECIMAL(10,2) NOT NULL,
    paid_eur          DECIMAL(10,2) NOT NULL DEFAULT 0,
    due_on_utc        DATETIME2 NOT NULL,
    paid_on_utc       DATETIME2 NULL,
    method            NVARCHAR(30) NULL,
    note              NVARCHAR(255) NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_payment_participation FOREIGN KEY (participation_id) REFERENCES fs_participation(participation_id)
);
GO

-- =============================================
-- Table: fs_referral
-- Description: Referral tracking for discounts
-- =============================================
CREATE TABLE fs_referral (
    referral_id       BIGINT IDENTITY(1,1) PRIMARY KEY,
    referrer_participation_id BIGINT NOT NULL,
    referred_participation_id BIGINT NOT NULL,
    discount_eur      DECIMAL(10,2) NOT NULL DEFAULT 5.00,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_referral_referrer FOREIGN KEY (referrer_participation_id) REFERENCES fs_participation(participation_id),
    CONSTRAINT FK_referral_referred FOREIGN KEY (referred_participation_id) REFERENCES fs_participation(participation_id),
    CONSTRAINT UQ_referral UNIQUE(referrer_participation_id, referred_participation_id)
);
GO

-- =============================================
-- Table: fs_prize_rule
-- Description: Prize rules configuration
-- =============================================
CREATE TABLE fs_prize_rule (
    prize_rule_id     INT IDENTITY(1,1) PRIMARY KEY,
    season_id         INT NOT NULL,
    type              NVARCHAR(20) NOT NULL, -- WEEKLY, FINAL, MIDSEASON
    min_players       INT NOT NULL,
    max_players       INT NOT NULL,
    placement         INT NULL,
    amount_eur        DECIMAL(10,2) NOT NULL,
    -- Audit fields
    data_inserimento  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    data_modifica     DATETIME2 NULL,
    utente_inserimento NVARCHAR(255) NOT NULL DEFAULT 'SYSTEM',
    utente_modifica   NVARCHAR(255) NULL,
    CONSTRAINT FK_prize_rule_season FOREIGN KEY (season_id) REFERENCES fs_season(season_id)
);
GO

-- =============================================
-- End of Schema Creation
-- =============================================
