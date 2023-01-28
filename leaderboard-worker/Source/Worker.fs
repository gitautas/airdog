namespace leaderboard_worker

open Main
open System
open System.Threading
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Google.Protobuf
open Grpc.Net.Client

type LeaderboardWorker(logger: ILogger<LeaderboardWorker>, account: Account) =
    interface IHostedService with
        member _.StartAsync(ct: CancellationToken) =
            task {
                use channel = GrpcChannel.ForAddress("http://server:8081/")
                let client = Airdog.AirdogClient(channel)

                let helper = new SteamLeaderboardHelper(account, "LEVELS", ct)
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)
                helper.Connect
                helper.Login
                let leaderboards = helper.GetEntries

                for leaderboard in leaderboards do
                    let req =
                        { UpdateLeaderboardReq.empty () with
                            LeaderboardId = leaderboard.ID
                            LevelId = leaderboard.LevelId
                            Entries = Collections.RepeatedField<Entry>() }

                    leaderboard.Entries
                    |> Seq.map (fun e ->
                        req.Entries.Add
                            { Entry.empty () with
                                SteamId = e.SteamID.ConvertToUInt64()
                                SteamRank = e.GlobalRank
                                TimeMs = e.Score })
                    |> ignore

                    try
                        client.UpdateLeaderboard(req) |> ignore
                    with ex ->
                        logger.LogWarning $"Failed to send leaderboard to server with {ex}, retrying..."

                        try
                            Thread.Sleep(2000)
                            client.UpdateLeaderboard(req) |> ignore
                        with ex ->
                            logger.LogError $"Failed to send leaderboard to server with {ex}."
                            raise ex

                logger.LogInformation "Succesfully finished task!"
            }

        member _.StopAsync(_: CancellationToken) =
            task { logger.LogInformation("Task cancelled") }
