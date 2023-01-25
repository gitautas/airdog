package service

import (
	"airdog/pkg/repository"
)

type AirdogService struct {
	db repository.Databaser
}

func CreateService(dbRepo *repository.DatabaseRepository) (*AirdogService, error) {
	return &AirdogService{
		db: dbRepo,
	}, nil
}
