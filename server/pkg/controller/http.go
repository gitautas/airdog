package controller

import (
	"airdog/pkg/service"
	"log"
	"strconv"

	"github.com/gofiber/fiber/v2"
)

type HttpController struct {
	app *fiber.App
	service service.Servicer
}

func CreateHttpController(service service.Servicer) (*HttpController, error) {
	app := fiber.New(fiber.Config {
		DisableStartupMessage: true})

	app.Get("/ping", pingHandler)

	return &HttpController{
		app:  app,
		service: service,
	}, nil
}

func pingHandler(c *fiber.Ctx) error {
	return c.SendString("pong")
}

func (c *HttpController) Serve(port int) error {
	log.Println("Listening for HTTP requests on ", port)
	return c.app.Listen(":" + strconv.Itoa(port))
}
