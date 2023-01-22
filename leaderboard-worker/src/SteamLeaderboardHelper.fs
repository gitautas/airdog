namespace leaderboard_worker

open System
open System.IO
open System.Threading
open Microsoft.Extensions.Logging
open SteamKit2


type Entry =
    { SteamID: uint64
      Time: int
      SteamRank: int }


type SteamLeaderboardHelper(account: Account, levelFile: string, ct: CancellationToken) =
    let neonWhiteSteamid = 1533420

    let logger =
        LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore).CreateLogger "SteamLeaderboardHelper"

    let mutable isDisposed = false
    let client = SteamClient account.Username // TODO: Implement an ISteamConfigurationBuilder to set a longer connection timeout
    let manager = CallbackManager client

    let user = client.GetHandler<SteamUser>()
    let userStats = client.GetHandler<SteamUserStats>()

    let levels = File.ReadLines levelFile |> Array.ofSeq


    let connectionSemaphore = new SemaphoreSlim(0, 1)
    let onConnected (_: SteamClient.ConnectedCallback) =
        logger.LogInformation("Connected to Steam network!")
        connectionSemaphore.Release() |> ignore
        async {
            ct.WaitHandle.WaitOne() |> ignore
            client.Disconnect()
        } |> Async.Start

    let onDisconnected (_: SteamClient.DisconnectedCallback) =
        logger.LogInformation("Disconnected from Steam network.")

    let watchCallbacks =
        async {
            logger.LogInformation("Starting to watch SteamKit callbacks.")

            while not ct.IsCancellationRequested do
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1))
        }

    do
        manager.Subscribe<SteamClient.ConnectedCallback> onConnected |> ignore
        manager.Subscribe<SteamClient.DisconnectedCallback> onDisconnected |> ignore
        watchCallbacks |> Async.Start |> ignore

    member _.Connect =
        logger.LogInformation("Connecting to Steam network...")
        client.Connect()
        connectionSemaphore.Wait()

    member _.Disconnect =
        logger.LogInformation("Disconnecting to Steam network...")
        client.Disconnect()

    member _.Login =
        let details = new SteamUser.LogOnDetails()
        details.Username <- account.Username
        details.Password <- account.Password
        user.LogOn details

    interface IDisposable with
        member this.Dispose() =
            this.Disconnect
            isDisposed <- true
