@_default:
  just --list

@build:
  docker compose build

@run:
  docker compose up

@proto:
  just _proto-go
  just _proto-fs

_proto-go:
  echo "Generating Go protobuf code..."
  protoc --go_out=./server/pkg/pb \
  --go-grpc_out=./server/pkg/pb \
  --go_opt=paths=source_relative \
  --go-grpc_opt=paths=source_relative \
  --proto_path=$HOME/Projects/airdog/proto \
  $HOME/Projects/airdog/proto/airdog.proto

_proto-fs:
  echo "Generating F# protobuf code..."
  protoc --fsharp_out=./leaderboard-worker/Source/Generated \
  --proto_path=$HOME/Projects/airdog/proto \
  $HOME/Projects/airdog/proto/airdog.proto
