CREATE TABLE leaderboards (
    steam_id bigint PRIMARY KEY,
    leaderboard_id int NOT NULL,
    steam_rank int,
    time_ms int,
);
