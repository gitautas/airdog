package controller

import (
	"airdog/pkg/pb"
	"airdog/pkg/service"
	"fmt"
	"io"
	"log"
	"net"
	"time"

	"google.golang.org/grpc"
)

type GrpcServer struct {
	service *service.AirdogService
	server *grpc.Server
    pb.UnimplementedAirdogServer
}

func CreateGrpcServer(service *service.AirdogService) (*GrpcServer, error) {
	server := grpc.NewServer()

	s := &GrpcServer{
		service: service,
		server: server,
	}

	pb.RegisterAirdogServer(server, s)

	return s, nil
}

func (g *GrpcServer) SyncEntries(stream pb.Airdog_SyncEntriesServer) error {
	startTime := time.Now()
	for {
		leaderboardEntries, err := stream.Recv()
		if err == io.EOF {
			endTime := time.Now()
			log.Println("Finished syncing leaderboards, took ", endTime.Sub(startTime).Seconds(), " seconds.")
			return stream.SendAndClose(&pb.SyncEntriesResp{})
		}
		if err != nil {
			return err
		}

		for _, entry := range leaderboardEntries.Entry {
			log.Println("Entry: ", entry.SteamId, " - ", entry.TimeMs)
		}
	}
}

func (g *GrpcServer) Serve(port int) error {
	log.Println("Listening for gRPC requests on ", port)
	lis, err := net.Listen("tcp", fmt.Sprintf("localhost:%d", port))
	if err != nil {
		return err
	}
	return g.server.Serve(lis)
}
