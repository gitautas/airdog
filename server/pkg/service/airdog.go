package service

import (
	"airdog/pkg/models"
	"airdog/pkg/repository"
)

type Servicer interface {
    GetLeaderboard(leaderboardId int) (leaderboard *models.Leaderboard, err error)

    GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error)
    AddEntry(entry *models.Entry) (err error)
    DeleteEntry(leaderboardId, steamId uint64) (err error)
	UpdateEntry(entry *models.Entry) (err error)
}


type AirdogService struct {
	db repository.LeaderboardDatabaser
}

func CreateService(dbRepo *repository.DatabaseRepository) (*AirdogService, error) {
	return &AirdogService{
		db: dbRepo,
	}, nil
}
