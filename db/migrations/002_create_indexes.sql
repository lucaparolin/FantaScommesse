-- =============================================
-- FantaScommesse Database Indexes
-- Version: 1.0
-- Description: Indexes for performance optimization
-- =============================================

-- Index on fs_user for email lookups (login)
CREATE INDEX IX_user_email ON fs_user(email);
GO

-- Index on fs_user for status filtering
CREATE INDEX IX_user_status ON fs_user(status) WHERE status = 1;
GO

-- Index on fs_season for active seasons
CREATE INDEX IX_season_is_closed ON fs_season(is_closed) WHERE is_closed = 0;
GO

-- Index on fs_round for season rounds
CREATE INDEX IX_round_season ON fs_round(season_id, round_no);
GO

-- Index on fs_round for deadline filtering
CREATE INDEX IX_round_deadline ON fs_round(deadline_utc);
GO

-- Index on fs_match for round lookups
CREATE INDEX IX_match_round ON fs_match(round_id, order_no);
GO

-- Index on fs_match for team lookups
CREATE INDEX IX_match_home_team ON fs_match(home_team_id);
CREATE INDEX IX_match_away_team ON fs_match(away_team_id);
GO

-- Index on fs_participation for user lookups
CREATE INDEX IX_participation_user ON fs_participation(user_id);
GO

-- Index on fs_participation for season lookups
CREATE INDEX IX_participation_season ON fs_participation(season_id);
GO

-- Index on fs_prediction for participation lookups
CREATE INDEX IX_prediction_participation ON fs_prediction(participation_id);
GO

-- Index on fs_prediction for round lookups
CREATE INDEX IX_prediction_round ON fs_prediction(round_id);
GO

-- Index on fs_prediction for submitted predictions
CREATE INDEX IX_prediction_submitted ON fs_prediction(submitted_utc) WHERE submitted_utc IS NOT NULL;
GO

-- Index on fs_prediction_item for prediction lookups
CREATE INDEX IX_prediction_item_prediction ON fs_prediction_item(prediction_id);
GO

-- Index on fs_prediction_item for match lookups
CREATE INDEX IX_prediction_item_match ON fs_prediction_item(match_id);
GO

-- Index on fs_score for prediction lookups
CREATE INDEX IX_score_prediction ON fs_score(prediction_id);
GO

-- Index on fs_bonus_event for round lookups
CREATE INDEX IX_bonus_event_round ON fs_bonus_event(round_id);
GO

-- Index on fs_bonus_event for participation lookups
CREATE INDEX IX_bonus_event_participation ON fs_bonus_event(participation_id);
GO

-- Index on fs_penalty_event for prediction lookups
CREATE INDEX IX_penalty_event_prediction ON fs_penalty_event(prediction_id);
GO

-- Index on fs_payment for participation lookups
CREATE INDEX IX_payment_participation ON fs_payment(participation_id);
GO

-- Index on fs_payment for due date filtering
CREATE INDEX IX_payment_due_on ON fs_payment(due_on_utc);
GO

-- Index on fs_referral for referrer lookups
CREATE INDEX IX_referral_referrer ON fs_referral(referrer_participation_id);
GO

-- Index on fs_referral for referred lookups
CREATE INDEX IX_referral_referred ON fs_referral(referred_participation_id);
GO

-- Index on fs_prize_rule for season lookups
CREATE INDEX IX_prize_rule_season ON fs_prize_rule(season_id);
GO

-- =============================================
-- End of Index Creation
-- =============================================
