-- =============================================
-- FantaScommesse Database Seed Data
-- Version: 1.0
-- Description: Sample data for development and testing
-- =============================================

-- Insert Admin User
INSERT INTO fs_user (email, password_hash, display_name, is_admin, is_organizer, utente_inserimento)
VALUES ('admin@fantascommesse.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Admin', 1, 1, 'SEED');
-- Password: Admin123! (BCrypt hashed)

-- Insert Organizer User
INSERT INTO fs_user (email, password_hash, display_name, is_organizer, utente_inserimento)
VALUES ('organizer@fantascommesse.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Organizer', 1, 'SEED');

-- Insert Test Users (Participants)
INSERT INTO fs_user (email, password_hash, display_name, utente_inserimento)
VALUES
    ('user1@test.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Mario Rossi', 'SEED'),
    ('user2@test.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Luigi Bianchi', 'SEED'),
    ('user3@test.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Giuseppe Verdi', 'SEED'),
    ('user4@test.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Anna Neri', 'SEED'),
    ('user5@test.it', '$2a$11$AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYz', 'Marco Ferrari', 'SEED');
GO

-- Insert Serie A Teams
INSERT INTO fs_team (name, utente_inserimento)
VALUES
    ('Inter', 'SEED'),
    ('AC Milan', 'SEED'),
    ('Juventus', 'SEED'),
    ('Napoli', 'SEED'),
    ('AS Roma', 'SEED'),
    ('Lazio', 'SEED'),
    ('Atalanta', 'SEED'),
    ('Fiorentina', 'SEED'),
    ('Bologna', 'SEED'),
    ('Torino', 'SEED'),
    ('Udinese', 'SEED'),
    ('Sassuolo', 'SEED'),
    ('Empoli', 'SEED'),
    ('Hellas Verona', 'SEED'),
    ('Monza', 'SEED'),
    ('Genoa', 'SEED'),
    ('Lecce', 'SEED'),
    ('Cagliari', 'SEED'),
    ('Frosinone', 'SEED'),
    ('Salernitana', 'SEED');
GO

-- Insert Season 2025/26
DECLARE @AdminUserId BIGINT = (SELECT user_id FROM fs_user WHERE email = 'admin@fantascommesse.it');

INSERT INTO fs_season (name, year_start, year_end, signup_deadline, base_fee_eur, referral_discount, created_by, utente_inserimento)
VALUES ('Serie A 2025/26', 2025, 2026, '2025-08-15 23:59:59', 100.00, 5.00, @AdminUserId, 'SEED');
GO

-- Insert Round 1
DECLARE @SeasonId INT = (SELECT season_id FROM fs_season WHERE name = 'Serie A 2025/26');

INSERT INTO fs_round (season_id, round_no, deadline_utc, is_published, utente_inserimento)
VALUES (@SeasonId, 1, '2025-08-25 18:00:00', 1, 'SEED');
GO

-- Insert Matches for Round 1
DECLARE @RoundId INT = (SELECT round_id FROM fs_round WHERE season_id = (SELECT season_id FROM fs_season WHERE name = 'Serie A 2025/26') AND round_no = 1);
DECLARE @Inter INT = (SELECT team_id FROM fs_team WHERE name = 'Inter');
DECLARE @Milan INT = (SELECT team_id FROM fs_team WHERE name = 'AC Milan');
DECLARE @Juve INT = (SELECT team_id FROM fs_team WHERE name = 'Juventus');
DECLARE @Napoli INT = (SELECT team_id FROM fs_team WHERE name = 'Napoli');
DECLARE @Roma INT = (SELECT team_id FROM fs_team WHERE name = 'AS Roma');
DECLARE @Lazio INT = (SELECT team_id FROM fs_team WHERE name = 'Lazio');
DECLARE @Atalanta INT = (SELECT team_id FROM fs_team WHERE name = 'Atalanta');
DECLARE @Fiorentina INT = (SELECT team_id FROM fs_team WHERE name = 'Fiorentina');
DECLARE @Bologna INT = (SELECT team_id FROM fs_team WHERE name = 'Bologna');
DECLARE @Torino INT = (SELECT team_id FROM fs_team WHERE name = 'Torino');
DECLARE @Udinese INT = (SELECT team_id FROM fs_team WHERE name = 'Udinese');
DECLARE @Sassuolo INT = (SELECT team_id FROM fs_team WHERE name = 'Sassuolo');
DECLARE @Empoli INT = (SELECT team_id FROM fs_team WHERE name = 'Empoli');
DECLARE @Verona INT = (SELECT team_id FROM fs_team WHERE name = 'Hellas Verona');
DECLARE @Monza INT = (SELECT team_id FROM fs_team WHERE name = 'Monza');
DECLARE @Genoa INT = (SELECT team_id FROM fs_team WHERE name = 'Genoa');
DECLARE @Lecce INT = (SELECT team_id FROM fs_team WHERE name = 'Lecce');
DECLARE @Cagliari INT = (SELECT team_id FROM fs_team WHERE name = 'Cagliari');
DECLARE @Frosinone INT = (SELECT team_id FROM fs_team WHERE name = 'Frosinone');
DECLARE @Salernitana INT = (SELECT team_id FROM fs_team WHERE name = 'Salernitana');

INSERT INTO fs_match (round_id, order_no, home_team_id, away_team_id, kickoff_utc, utente_inserimento)
VALUES
    (@RoundId, 1, @Inter, @Genoa, '2025-08-25 20:45:00', 'SEED'),
    (@RoundId, 2, @Milan, @Bologna, '2025-08-26 18:30:00', 'SEED'),
    (@RoundId, 3, @Juve, @Udinese, '2025-08-26 20:45:00', 'SEED'),
    (@RoundId, 4, @Napoli, @Frosinone, '2025-08-27 18:30:00', 'SEED'),
    (@RoundId, 5, @Roma, @Salernitana, '2025-08-27 20:45:00', 'SEED'),
    (@RoundId, 6, @Lazio, @Lecce, '2025-08-28 18:30:00', 'SEED'),
    (@RoundId, 7, @Atalanta, @Torino, '2025-08-28 20:45:00', 'SEED'),
    (@RoundId, 8, @Fiorentina, @Monza, '2025-08-29 18:30:00', 'SEED'),
    (@RoundId, 9, @Empoli, @Verona, '2025-08-29 20:45:00', 'SEED'),
    (@RoundId, 10, @Sassuolo, @Cagliari, '2025-08-30 20:45:00', 'SEED');
GO

-- Insert Participations
DECLARE @SeasonId2 INT = (SELECT season_id FROM fs_season WHERE name = 'Serie A 2025/26');
DECLARE @User1 BIGINT = (SELECT user_id FROM fs_user WHERE email = 'user1@test.it');
DECLARE @User2 BIGINT = (SELECT user_id FROM fs_user WHERE email = 'user2@test.it');
DECLARE @User3 BIGINT = (SELECT user_id FROM fs_user WHERE email = 'user3@test.it');
DECLARE @User4 BIGINT = (SELECT user_id FROM fs_user WHERE email = 'user4@test.it');
DECLARE @User5 BIGINT = (SELECT user_id FROM fs_user WHERE email = 'user5@test.it');

INSERT INTO fs_participation (user_id, season_id, utente_inserimento)
VALUES
    (@User1, @SeasonId2, 'SEED'),
    (@User2, @SeasonId2, 'SEED'),
    (@User3, @SeasonId2, 'SEED'),
    (@User4, @SeasonId2, 'SEED'),
    (@User5, @SeasonId2, 'SEED');
GO

-- Insert Prize Rules
DECLARE @SeasonId3 INT = (SELECT season_id FROM fs_season WHERE name = 'Serie A 2025/26');

INSERT INTO fs_prize_rule (season_id, type, min_players, max_players, placement, amount_eur, utente_inserimento)
VALUES
    (@SeasonId3, 'WEEKLY', 10, 50, NULL, 20.00, 'SEED'),
    (@SeasonId3, 'FINAL', 10, 50, 1, 300.00, 'SEED'),
    (@SeasonId3, 'FINAL', 10, 50, 2, 150.00, 'SEED'),
    (@SeasonId3, 'FINAL', 10, 50, 3, 75.00, 'SEED');
GO

-- =============================================
-- End of Seed Data
-- =============================================
