package main

import (
	"airdog/pkg/controller"
	"airdog/pkg/repository"
	"airdog/pkg/service"
	"fmt"
	"log"
)

func main() {
	log.Println("Starting Airdog server...")

	dbUser := ""
	dbPass := ""
	dbPort := 5234

	conn := fmt.Sprint("postgresql://%s:%s@localhost:%i/airdog", dbUser, dbPass, dbPort)

	repo, err := repository.CreateDBRepository(conn)
	if err != nil {
		log.Fatalln("Failed to create database repository: ", err.Error())
	}

	service, err := service.CreateService(repo)
	if err != nil {
		log.Fatalln("Failed to create service: ", err.Error())
	}

	httpController, err := controller.CreateHttpController(service)
	if err != nil {
		log.Fatalln("Failed to create HTTP controller: ", err.Error())
	}
	grpcController, err := controller.CreateGrpcServer(service)
	if err != nil {
		log.Fatalln("Failed to create gRPC controller: ", err.Error())
	}

	go log.Fatalln(httpController.Serve(8080))
	go log.Fatalln(grpcController.Serve(8081))
}
