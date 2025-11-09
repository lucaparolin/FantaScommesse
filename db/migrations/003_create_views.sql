-- =============================================
-- FantaScommesse Database Views
-- Version: 1.0
-- Description: Useful views for reporting and scoreboards
-- =============================================

-- =============================================
-- View: vw_round_scoreboard
-- Description: Scoreboard for a specific round
-- =============================================
CREATE OR ALTER VIEW vw_round_scoreboard AS
SELECT
    r.round_id,
    r.season_id,
    r.round_no,
    p.participation_id,
    u.user_id,
    u.display_name,
    pr.prediction_id,
    pr.is_valid,
    pr.is_late,
    pr.submitted_utc,
    s.points_base,
    s.bonus,
    s.penalty,
    s.total,
    s.computed_utc,
    RANK() OVER (PARTITION BY r.round_id ORDER BY s.total DESC) as rank_position
FROM fs_round r
INNER JOIN fs_prediction pr ON pr.round_id = r.round_id
INNER JOIN fs_participation p ON p.participation_id = pr.participation_id
INNER JOIN fs_user u ON u.user_id = p.user_id
LEFT JOIN fs_score s ON s.prediction_id = pr.prediction_id
WHERE pr.submitted_utc IS NOT NULL;
GO

-- =============================================
-- View: vw_season_scoreboard
-- Description: Overall season scoreboard (cumulative)
-- =============================================
CREATE OR ALTER VIEW vw_season_scoreboard AS
SELECT
    s.season_id,
    p.participation_id,
    u.user_id,
    u.display_name,
    COUNT(DISTINCT pr.round_id) as rounds_played,
    SUM(sc.points_base) as total_base_points,
    SUM(sc.bonus) as total_bonus,
    SUM(sc.penalty) as total_penalty,
    SUM(sc.total) as total_points,
    RANK() OVER (PARTITION BY s.season_id ORDER BY SUM(sc.total) DESC) as rank_position
FROM fs_season s
INNER JOIN fs_participation p ON p.season_id = s.season_id
INNER JOIN fs_user u ON u.user_id = p.user_id
LEFT JOIN fs_prediction pr ON pr.participation_id = p.participation_id
LEFT JOIN fs_score sc ON sc.prediction_id = pr.prediction_id
WHERE pr.submitted_utc IS NOT NULL
GROUP BY s.season_id, p.participation_id, u.user_id, u.display_name;
GO

-- =============================================
-- View: vw_user_payment_status
-- Description: Payment status summary per user/season
-- =============================================
CREATE OR ALTER VIEW vw_user_payment_status AS
SELECT
    p.participation_id,
    p.user_id,
    u.display_name,
    p.season_id,
    se.name as season_name,
    SUM(pay.expected_eur) as total_expected,
    SUM(pay.paid_eur) as total_paid,
    SUM(pay.expected_eur - pay.paid_eur) as total_due,
    CASE
        WHEN SUM(pay.expected_eur - pay.paid_eur) <= 0 THEN 'PAID'
        WHEN SUM(pay.paid_eur) > 0 THEN 'PARTIAL'
        ELSE 'UNPAID'
    END as payment_status
FROM fs_participation p
INNER JOIN fs_user u ON u.user_id = p.user_id
INNER JOIN fs_season se ON se.season_id = p.season_id
LEFT JOIN fs_payment pay ON pay.participation_id = p.participation_id
GROUP BY p.participation_id, p.user_id, u.display_name, p.season_id, se.name;
GO

-- =============================================
-- View: vw_match_results
-- Description: Match results with team names
-- =============================================
CREATE OR ALTER VIEW vw_match_results AS
SELECT
    m.match_id,
    m.round_id,
    r.round_no,
    r.season_id,
    m.order_no,
    ht.name as home_team,
    at.name as away_team,
    m.kickoff_utc,
    m.result_code,
    m.goals_home,
    m.goals_away,
    m.ggng,
    m.ou
FROM fs_match m
INNER JOIN fs_round r ON r.round_id = m.round_id
INNER JOIN fs_team ht ON ht.team_id = m.home_team_id
INNER JOIN fs_team at ON at.team_id = m.away_team_id;
GO

-- =============================================
-- View: vw_prediction_detail
-- Description: Detailed prediction view with selections
-- =============================================
CREATE OR ALTER VIEW vw_prediction_detail AS
SELECT
    pr.prediction_id,
    pr.participation_id,
    u.display_name,
    pr.round_id,
    r.round_no,
    r.season_id,
    m.match_id,
    m.order_no,
    ht.name as home_team,
    at.name as away_team,
    pi.selection,
    m.result_code,
    CASE
        WHEN m.result_code IS NULL THEN NULL
        WHEN (pi.selection = '1' AND m.result_code = '1') THEN 1
        WHEN (pi.selection = 'X' AND m.result_code = 'X') THEN 1
        WHEN (pi.selection = '2' AND m.result_code = '2') THEN 1
        WHEN (pi.selection = '1X' AND m.result_code IN ('1','X')) THEN 1
        WHEN (pi.selection = 'X2' AND m.result_code IN ('X','2')) THEN 1
        WHEN (pi.selection = '12' AND m.result_code IN ('1','2')) THEN 1
        WHEN (pi.selection = 'GG' AND m.ggng = 'GG') THEN 1
        WHEN (pi.selection = 'NG' AND m.ggng = 'NG') THEN 1
        WHEN (pi.selection = 'OVER' AND m.ou = 'OVER') THEN 1
        WHEN (pi.selection = 'UNDER' AND m.ou = 'UNDER') THEN 1
        ELSE 0
    END as is_correct
FROM fs_prediction pr
INNER JOIN fs_participation p ON p.participation_id = pr.participation_id
INNER JOIN fs_user u ON u.user_id = p.user_id
INNER JOIN fs_round r ON r.round_id = pr.round_id
INNER JOIN fs_prediction_item pi ON pi.prediction_id = pr.prediction_id
INNER JOIN fs_match m ON m.match_id = pi.match_id
INNER JOIN fs_team ht ON ht.team_id = m.home_team_id
INNER JOIN fs_team at ON at.team_id = m.away_team_id;
GO

-- =============================================
-- End of View Creation
-- =============================================
