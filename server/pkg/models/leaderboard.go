package models

type Entry struct {
	LeaderboardId int `json:"leaderboardId"`
	SteamID uint64 `json:"steamId"`
	SteamRank int `json:"steamRank"`
	Time int `json:"time"`
	Rank int `json:"rank"`
}

type Leaderboard struct {
	ID int `json:"id"`
	LevelID string `json:"levelId"`
	EntryCount int `json:"entryCount"`
	Entries []Entry `json:"entries"`
}
