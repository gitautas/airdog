package controller

import (
	"airdog/pkg/pb"
	"airdog/pkg/service"
	"context"
	"fmt"
	"log"
	"net"

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

func (g *GrpcServer) UpdateLeaderboard(ctx context.Context, req *pb.UpdateLeaderboardReq) (*pb.UpdateLeaderboardResp, error) {
	return &pb.UpdateLeaderboardResp{}, nil
}

func (g *GrpcServer) Serve(port int) error {
	log.Println("Listening for gRPC requests on ", port)
	lis, err := net.Listen("tcp", fmt.Sprintf("localhost:%d", port))
	if err != nil {
		return err
	}
	return g.server.Serve(lis)
}
