package repository

import (
	"airdog/pkg/models"
	"context"
	"database/sql"

	_ "github.com/lib/pq"
)

type LeaderboardDatabaser interface {
    GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error)
    AddEntry(entry *models.Entry) (err error)
    DeleteEntry(leaderboardId, steamId uint64) (err error)
	UpdateEntry(entry *models.Entry) (err error)

    GetLeaderboard(leaderboardId int) (leaderboard *models.Leaderboard, err error)
	UpdateLeaderboard(leaderboard *models.Leaderboard) (err error)
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

func (db *DatabaseRepository) GetLeaderboard(leaderboardId int) (leaderboard *models.Leaderboard, err error) {
	rows, err := db.db.Query("SELECT * FROM leaderboards WHERE leaderboard_id = ?", leaderboardId)
	if err != nil {
		return nil, err
	}

	defer rows.Close()

	leaderboard = &models.Leaderboard{
		ID:         leaderboardId,
		Entries:    []*models.Entry{},
	}

	for rows.Next() {
		entry := &models.Entry{
			LeaderboardId: leaderboardId,
		}
		err =  rows.Scan(&entry)
		if err != nil {
			return nil, err
		}

		leaderboard.Entries = append(leaderboard.Entries, entry)
	}
	return leaderboard, err
}

func (db *DatabaseRepository) GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error) {
	row := db.db.QueryRow("SELECT * FROM leaderboards WHERE leaderboard_id = ? AND steam_id = ?", leaderboardId, steamId)
	err = row.Scan(&entry)
	return entry, err
}

func (db *DatabaseRepository) AddEntry(entry *models.Entry) (err error) {
	_, err = db.db.Exec("INSERT INTO leaderboards(leaderboard_id, steam_id, steam_rank, time_ms, created_at)" +
		"VALUES(?, ?, ?, ?, NOW())", entry.LeaderboardId,
		entry.SteamID, entry.SteamRank, entry.Time)

	return err
}

func (db *DatabaseRepository) DeleteEntry(leaderboardId, steamId uint64) (err error) {
	_, err = db.db.Exec("DELETE * FROM leaderboards WHERE leaderboard_id = ? AND steam_id = ?", leaderboardId, steamId)

	return err
}

func (db *DatabaseRepository) UpdateEntry(entry *models.Entry) (err error) {
	_, err = db.db.Exec("UPDATE leaderboards SET(steam_id, steam_rank, time_ms, updated_at)" +
		"VALUES(?, ?, ?, NOW()) WHERE id = ?",
		entry.SteamID, entry.SteamRank, entry.Time, entry.LeaderboardId)

	return err
}

func (db *DatabaseRepository) UpdateLeaderboard(leaderboard *models.Leaderboard) (err error) {
	tx, err := db.db.BeginTx(context.Background(), nil)
	if err != nil {
		return err
	}

	_, err = db.db.Exec("DELETE * FROM leaderboards WHERE id = ?", leaderboard.ID)

	for _, entry := range leaderboard.Entries {
		err = db.AddEntry(entry)
		if err != nil {
			tx.Rollback()
			return err
		}
	}

	return tx.Commit()
}
