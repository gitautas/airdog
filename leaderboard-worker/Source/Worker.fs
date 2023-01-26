namespace leaderboard_worker

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Google.Protobuf
open Grpc.Net.Client

type LeaderboardWorker(logger: ILogger<LeaderboardWorker>, account: Account) =
    interface IHostedService with
        member _.StartAsync(ct: CancellationToken) =
            task {
                use channel = GrpcChannel.ForAddress("http://localhost:8081/")
                let client = Airdog.AirdogService.AirdogServiceClient(channel)

                let helper = new SteamLeaderboardHelper(account, "LEVELS", ct)
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)
                helper.Connect
                helper.Login
                let leaderboards = helper.GetEntries

                for leaderboard in leaderboards do
                    let req = { LeaderboardId = leaderboard.ID; Entry = leaderboard.Entries }
                    printfn $"Entries for leaderboard {leaderboard.ID}"
                    for entry in leaderboard.Entries do
                        printfn $"SteamID: {entry.SteamID.ConvertToUInt64()} - {entry.Score}ms"

                logger.LogInformation "Succesfully finished task!"
            }

        member _.StopAsync(ct: CancellationToken) =
            task { logger.LogInformation("Task cancelled") }
