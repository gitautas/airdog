package models

type Entry struct {
	SteamID uint64 `json:"steamId"`
	LeaderboardId int `json:"leaderboardId"`
	LevelId string `json:"levelId"`
	SteamRank int `json:"steamRank"`
	Time int `json:"time"`

	Rank int `json:"rank"`
}

type Leaderboard struct {
	ID int `json:"id"`
	EntryCount int `json:"entryCount"`
	Entries []*Entry `json:"entries"`
}
