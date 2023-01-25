namespace leaderboard_worker

open System.Threading
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
        let rate_timer = // Amount of minutes to wait between interations
            100000 // Steam's global daily API rate limit
            / 300  // Apporoximate amount of requests to query all leaderboards
            / 24   // Runninig 24/7


        // This interface returns a sequence of
        // Steam usernames and passwords. This is
        // used for autoscaling.
        let account_provider: IAccountProvider = FileAccountProvider "STEAM_ACCOUNTS" // This implementation reads
        // entries from a file
        let builders =
            account_provider.GetAccounts
            |> Seq.map (fun account -> (createHostBuilder account))

        while true do
            for builder in builders do
                builder.Build().Run()
                Thread.Sleep(rate_timer / Seq.length(builders) * 60000)

        0 // exit code
