package service

import (
	"airdog/pkg/models"
	"airdog/pkg/repository"
)

type Servicer interface {
    GetLeaderboard(leaderboardId int) (leaderboard *models.Leaderboard, err error)
	UpdateLeaderboard(leaderboard *models.Leaderboard) (err error)

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

func (s *AirdogService) GetLeaderboard(leaderboardId int) (leaderboard *models.Leaderboard, err error) {
	leaderboard, err = s.db.GetLeaderboard(leaderboardId)
	if err != nil {
		return nil, err
	}

	return leaderboard, nil
}

func (s *AirdogService) UpdateLeaderboard(leaderboard *models.Leaderboard) (err error) {
	err = s.db.UpdateLeaderboard(leaderboard)

	return err
}


func (s *AirdogService) GetEntry(leaderboardId int, steamId uint64) (entry *models.Entry, err error) {
	entry, err = s.db.GetEntry(leaderboardId, steamId)
	if err != nil {
		return nil, err
	}

	return entry, nil
}

func (s *AirdogService) AddEntry(entry *models.Entry) (err error) {
	err = s.db.AddEntry(entry)

	return err
}

func (s *AirdogService) DeleteEntry(leaderboardId, steamId uint64) (err error) {
	err = s.db.DeleteEntry(leaderboardId, steamId)

	return err

}

func (s *AirdogService) UpdateEntry(entry *models.Entry) (err error) {
	err = s.db.UpdateEntry(entry)

	return err
}
