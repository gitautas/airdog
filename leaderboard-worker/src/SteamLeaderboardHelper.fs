namespace leaderboard_worker

open System
open System.IO
open Microsoft.Extensions.Logging
open SteamKit2


type Entry = { SteamID: uint64; Time: int; SteamRank: int }


type SteamLeaderboardHelper(account: Account, levelFile: string) =
    let neonWhiteSteamid = 1533420
    let logger = LoggerFactory.Create(fun builder ->
                                      builder.AddConsole()
                                      |> ignore).CreateLogger "SteamLeaderboardHelper"

    let mutable isDisposed = false
    let client = SteamClient account.Username  // TODO: Implement an ISteamConfigurationBuilder to set a longer connection timeout
    let manager = CallbackManager client

    let user = client.GetHandler<SteamUser>()
    let userStats = client.GetHandler<SteamUserStats>()

    let levels = File.ReadLines levelFile |> Array.ofSeq



    let onConnected (c: SteamClient.ConnectedCallback) =
        logger.LogInformation("Connected to Steam network!")

    let onDisconnected (c: SteamClient.DisconnectedCallback) =
        logger.LogInformation("Disconnected from Steam network.")

    let watchCallbacks =
        async {
            logger.LogInformation("Starting to watch SteamKit callbacks.")
            while not isDisposed do manager.RunWaitCallbacks(TimeSpan.FromSeconds(1))
        }

    do
        manager.Subscribe<SteamClient.ConnectedCallback> onConnected |> ignore
        manager.Subscribe<SteamClient.DisconnectedCallback> onDisconnected |> ignore
        watchCallbacks |> ignore

    member _.Connect =
        logger.LogInformation("Connecting to Steam network...")
        client.Connect

    member _.Login =
        let details = new SteamUser.LogOnDetails()
        details.Username <- account.Username
        details.Password <- account.Password
        user.LogOn details

    interface IDisposable with
        member _.Dispose() =
            isDisposed <- true
