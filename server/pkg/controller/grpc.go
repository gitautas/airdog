package controller

import (
	"airdog/pkg/models"
	"airdog/pkg/pb"
	"airdog/pkg/service"
	"context"
	"fmt"
	"log"
	"net"

	"google.golang.org/grpc"
)

type GrpcServer struct {
	service service.Servicer
	server *grpc.Server
    pb.UnimplementedAirdogServer
}

func CreateGrpcServer(service service.Servicer) (*GrpcServer, error) {
	server := grpc.NewServer()

	s := &GrpcServer{
		service: service,
		server: server,
	}

	pb.RegisterAirdogServer(server, s)

	return s, nil
}

func (g *GrpcServer) UpdateLeaderboard(ctx context.Context, req *pb.UpdateLeaderboardReq) (*pb.UpdateLeaderboardResp, error) {
	leaderboard := &models.Leaderboard{
		ID:         int(req.LeaderboardId),
		Entries:    []*models.Entry{},
	}

	for _, entry := range req.Entry {
		leaderboard.Entries = append(leaderboard.Entries, &models.Entry{
			LeaderboardId: int(req.LeaderboardId),
			SteamID:       entry.SteamId,
			SteamRank:     int(entry.SteamRank),
			Time:          int(entry.TimeMs),
		})
	}

	leaderboard.EntryCount = len(leaderboard.Entries)

	err := g.service.UpdateLeaderboard(leaderboard)

	return &pb.UpdateLeaderboardResp{}, err
}

func (g *GrpcServer) Serve(port int) error {
	log.Println("Listening for gRPC requests on ", port)
	lis, err := net.Listen("tcp", fmt.Sprintf("localhost:%d", port))
	if err != nil {
		return err
	}
	return g.server.Serve(lis)
}
