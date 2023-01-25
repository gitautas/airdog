namespace leaderboard_worker

open System
open System.IO
open System.Threading
open System.Collections.ObjectModel
open Microsoft.Extensions.Logging
open SteamKit2

type Leaderboard =
    { ID: int
      Entries: ReadOnlyCollection<SteamUserStats.LeaderboardEntriesCallback.LeaderboardEntry> }

type SteamLeaderboardHelper(account: Account, levelFile: string, ct: CancellationToken) =
    let neonWhiteSteamid = 1533420u

    let logger =
        LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore).CreateLogger "SteamLeaderboardHelper"

    let client = SteamClient account.Username // TODO: Implement an ISteamConfigurationBuilder to set a longer connection timeout
    let manager = CallbackManager client

    let user = client.GetHandler<SteamUser>()
    let userStats = client.GetHandler<SteamUserStats>()

    let levels = File.ReadLines levelFile |> Array.ofSeq

    let connectionSemaphore = new SemaphoreSlim(0, 1)

    let onConnected (_: SteamClient.ConnectedCallback) =
        logger.LogInformation "Connected to Steam network!"
        connectionSemaphore.Release() |> ignore

    let onDisconnected (_: SteamClient.DisconnectedCallback) =
        logger.LogInformation "Disconnected from Steam network."

    let logOnSemaphore = new SemaphoreSlim(0, 1)

    let onLoggedOn (c: SteamUser.LoggedOnCallback) =
        logger.LogInformation $"Logged on to user: {c.Result}"
        logOnSemaphore.Release() |> ignore

    let onLoggedOff (_: SteamUser.LoggedOffCallback) =
        logger.LogInformation $"Logged off from user {user.SteamID.ConvertToUInt64()}"


    let watchCallbacks =
        async {
            logger.LogInformation "Starting to watch SteamKit callbacks."

            while not ct.IsCancellationRequested do
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1))
        }

    do
        manager.Subscribe<SteamClient.ConnectedCallback> onConnected |> ignore
        manager.Subscribe<SteamClient.DisconnectedCallback> onDisconnected |> ignore
        manager.Subscribe<SteamUser.LoggedOnCallback> onLoggedOn |> ignore
        manager.Subscribe<SteamUser.LoggedOffCallback> onLoggedOff |> ignore
        watchCallbacks |> Async.Start |> ignore

    member _.Connect =
        logger.LogInformation "Connecting to Steam network..."
        client.Connect()
        connectionSemaphore.Wait()

    member _.Disconnect =
        logger.LogInformation "Disconnecting to Steam network..."
        client.Disconnect()

    member _.Login =
        let details = new SteamUser.LogOnDetails()
        details.Username <- account.Username
        details.Password <- account.Password
        logger.LogInformation ("Logging on to user {}:{}", details.Username, details.Password)
        user.LogOn details
        logOnSemaphore.Wait()

    member _.GetEntries =
        levels
        |> Array.Parallel.map (fun levelId ->
            let leaderboard =
                userStats.FindLeaderboard(neonWhiteSteamid, levelId).GetAwaiter().GetResult
                |> (fun f ->
                        try f()
                        with ex ->
                            logger.LogWarning $"Failed to find leaderboard for level {levelId}: {ex}, retrying..."
                            Thread.Sleep(2000)
                            try f()
                            with ex ->
                                logger.LogError $"Failed to find leaderboard for level {levelId}: {ex}."
                                raise ex)

            let result = userStats.GetLeaderboardEntries(
                    neonWhiteSteamid,
                    leaderboard.ID,
                    0,
                    leaderboard.EntryCount,
                    ELeaderboardDataRequest.Global).GetAwaiter().GetResult
            try result() |> (fun (r) -> {ID = leaderboard.ID; Entries = r.Entries})
            with ex ->
                logger.LogWarning $"Failed to process entries for leaderboard {leaderboard.ID} with exception {ex}, retrying..."
                try
                    Thread.Sleep(2000)
                    result() |> (fun (r) -> {ID = leaderboard.ID; Entries = r.Entries})
                with ex ->
                    logger.LogError $"Failed to process entries for leaderboard {leaderboard.ID} with exception {ex}."
                    raise ex)

    // interface IDisposable with
    //     member this.Dispose() =
    //         logger.LogInformation "Destroying leaderboard helper..."
    //         this.Disconnect
