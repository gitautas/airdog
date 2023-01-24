namespace leaderboard_worker

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

type LeaderboardWorker(logger: ILogger<LeaderboardWorker>, account: Account) =
    interface IHostedService with
        member _.StartAsync(ct: CancellationToken) =
            task {
                let helper = new SteamLeaderboardHelper(account, "LEVELS", ct)
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)
                helper.Connect
                helper.Login
                let entries = helper.GetEntries

                entries
            }

        member _.StopAsync(ct: CancellationToken) =
            task { logger.LogInformation("Task cancelled") }
