namespace leaderboard_worker

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module Program =
    let createHostBuilder account =
        Host
            .CreateDefaultBuilder()
            .ConfigureServices(fun hostContext services ->
                services.AddHostedService(fun serviceProvider ->
                    new LeaderboardWorker(serviceProvider.GetService<ILogger<LeaderboardWorker>>(), account))
                |> ignore)

    [<EntryPoint>]
    let main args =
        // Steam's global rate limit
        // let rate_limit = 100000

        // This interface returns a sequence of
        // Steam usernames and passwords. This is
        // used for autoscaling.
        let account_provider: IAccountProvider = FileAccountProvider "STEAM_ACCOUNTS" // This implementation reads
        // entries from a file
        let builders =
            account_provider.GetAccounts
            |> Seq.map (fun account -> createHostBuilder account)

        for builder in builders do
            builder.Build().RunAsync() |> ignore

        while true do
            ()

        0 // exit code
