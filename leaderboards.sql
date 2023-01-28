CREATE DATABASE IF NOT EXISTS airdog;

USE airdog;

CREATE TABLE IF NOT EXISTS leaderboards (
    steam_id bigint PRIMARY KEY,
    leaderboard_id int NOT NULL,
    level_id varchar(32) NOT NULL,
    steam_rank int,
    time_ms int,
);
