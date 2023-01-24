namespace leaderboard_worker

open System.IO

// Represents steam account information.
// TODO: Add support for SteamGuard login
// and sentry file persistence.
type Account = { Username: string; Password: string }

// An interface for getting a sequence of key value pairs
// that are structured as "AccountName: Password" for
// Steam accounts.
type IAccountProvider =
    abstract member GetAccounts : seq<Account>

// This implementation reads name - password entries from a file.
// This is not a good way to do this, if you're hosting Airdog
// on a cloud provider, use their managed secrets management solution.
type public FileAccountProvider(path: string) =
    let filename = path
    interface IAccountProvider with
        member _.GetAccounts =
                File.ReadLines filename
                |> Seq.map
                    (fun v ->
                     let e = v.Split ":"
                     {Username = e[0]; Password = e[1]})
