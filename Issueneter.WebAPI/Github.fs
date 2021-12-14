module Github
    open Octokit
    open System.Threading.Tasks
    open System.Collections.Generic
    
    let private getIssues (client: GitHubClient) (filter: RepositoryIssueRequest) = client.Issue.GetAllForRepository("dotnet", "runtime", filter)

    let getIssueEvents (client: GitHubClient) (issue : Issue) = client.Issue.Timeline.GetAllForIssue("dotnet", "runtime", issue.Number)

    let getAllIssues (client: GitHubClient) (filters: seq<RepositoryIssueRequest>) : Task<IReadOnlyList<Issue>[]> = 
        filters
        |> Seq.map (fun (x: RepositoryIssueRequest) -> getIssues client x)
        |> Task.WhenAll