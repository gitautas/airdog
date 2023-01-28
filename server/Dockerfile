# syntax=docker/dockerfile:1

FROM golang:1.19 AS build
WORKDIR /build
COPY . .
RUN CGO_ENABLED=0 go build ./cmd/server.go

FROM alpine:latest
RUN apk --no-cache add ca-certificates
WORKDIR /app
COPY --from=build /build .
ENTRYPOINT ["/app/server"]
