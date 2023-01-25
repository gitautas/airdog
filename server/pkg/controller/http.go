package controller

import (
	"airdog/pkg/service"
	"log"
	"strconv"

	"github.com/gofiber/fiber/v2"
)

type HttpController struct {
	app *fiber.App
	service *service.AirdogService
}

func CreateHttpController(service *service.AirdogService) (*HttpController, error) {
	app := fiber.New()

	app.Get("/ping", pingHandler)

	return &HttpController{
		app:  app,
		service: service,
	}, nil
}

func (c *HttpController) Serve(port int) error {
	log.Println("Listening for HTTP requests on ", port)
	return c.app.Listen(":" + strconv.Itoa(port))
}

func pingHandler(c *fiber.Ctx) error {
	return c.SendString("pong")
}
