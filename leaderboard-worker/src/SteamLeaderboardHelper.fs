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

    let retry maxRetries before f =
        let rec loop retriesRemaining =
            try
                f ()
            with _ when retriesRemaining > 0 ->
                before ()
                loop (retriesRemaining - 1)

        loop maxRetries

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

    let onLoggedOn (_: SteamUser.LoggedOnCallback) =
        logger.LogInformation $"Logged on to user {user.SteamID.ConvertToUInt64}"
        logOnSemaphore.Release() |> ignore

    let onLoggedOff (_: SteamUser.LoggedOffCallback) =
        logger.LogInformation $"Logged off from user {user.SteamID.ConvertToUInt64}"


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
        user.LogOn details
        logOnSemaphore.Wait()

    member _.GetEntries =
        levels
        |> Array.Parallel.map (fun levelId ->
            let leaderboard =
                userStats.FindLeaderboard(neonWhiteSteamid, levelId).GetAwaiter().GetResult
                |> retry 3 (fun () ->
                    logger.LogWarning "FindLeaderboard request failed. Retrying..."
                    System.Threading.Thread.Sleep 2000)

            let entries =
                userStats
                    .GetLeaderboardEntries(
                        neonWhiteSteamid,
                        leaderboard.ID,
                        0,
                        leaderboard.EntryCount,
                        ELeaderboardDataRequest.Global
                    )
                    .GetAwaiter()
                    .GetResult
                |> retry 3 (fun () ->
                    logger.LogWarning "Failed"
                    System.Threading.Thread.Sleep 2000)

            { ID = leaderboard.ID
              Entries = entries.Entries })

    interface IDisposable with
        member this.Dispose() =
            logger.LogInformation "Destroying leaderboard helper..."
            this.Disconnect
