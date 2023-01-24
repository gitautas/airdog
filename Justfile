@_default:
  just --list

@build:
  docker compose build

@run:
  docker compose up

@proto:
  just _proto-go

_proto-go:
  echo "Generating go..."
  protoc --go_out=./server/pkg/pb \
  --go-grpc_out=./server/pkg/pb \
  --go_opt=paths=source_relative \
  --go-grpc_opt=paths=source_relative \
  --proto_path=$HOME/Projects/airdog/proto \
  $HOME/Projects/airdog/proto/airdog.proto
