package repository

import (
	"airdog/pkg/models"
	"database/sql"

	_ "github.com/lib/pq"
)

type Databaser interface {
    GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error)

    AddEntry(entry *models.Entry) (err error)

    DeleteEntry(leaderboardId, steamId uint64) (err error)

	UpdateEntry(entry *models.Entry) (err error)
}

type DatabaseRepository struct {
	db *sql.DB
}

func CreateDBRepository(conn string) (*DatabaseRepository, error) {
	db, err := sql.Open("postgres", conn)
	if err != nil {
		return nil, err
	}

	return &DatabaseRepository {
		db: db,
	}, nil
}

func (db *DatabaseRepository) GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error) {
	row := db.db.QueryRow("SELECT * FROM ? WHERE steam_id = ?", leaderboardId, steamId)
	err = row.Scan(&entry)
	return entry, err
}

func (db *DatabaseRepository) AddEntry(entry *models.Entry) (err error) {
	_, err = db.db.Exec("INSERT INTO ?(steam_id, steam_rank, time, created_at)" +
		"VALUES(?, ?, ?, NOW())", entry.LeaderboardId,
		entry.SteamID, entry.SteamRank, entry.Time)

	return err
}

func (db *DatabaseRepository) DeleteEntry(leaderboardId, steamId uint64) (err error) {
	_, err = db.db.Exec("DELETE * FROM ? WHERE steam_id = ?", leaderboardId, steamId)

	return err
}

func (db *DatabaseRepository) UpdateEntry(entry *models.Entry) (err error) {
	_, err = db.db.Exec("UPDATE ? SET(steam_id, steam_rank, time, updated_at)" +
		"VALUES(?, ?, ?, NOW())", entry.LeaderboardId,
		entry.SteamID, entry.SteamRank, entry.Time)

	return err
}
