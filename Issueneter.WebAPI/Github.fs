module Github
    open Octokit
    open System.Threading.Tasks
    open FSharp.Control.Tasks
    open System.Collections.Generic
    open IssueLabels

    type SearchedIssues = {
        Issues: IReadOnlyList<Issue>
        SearchLabel: EasyIssueLabel
    }

    let private getIssues (client: GitHubClient) (filter: SearchFilter) = task {
        let! issues = client.Issue.GetAllForRepository("dotnet", "runtime", filter.BaseFilter)
        return filter.Label, issues
    }

    let getIssueEvents (client: GitHubClient) (issue : Issue) = client.Issue.Timeline.GetAllForIssue("dotnet", "runtime", issue.Number)

    let getAllIssues (client: GitHubClient) (filters: seq<SearchFilter>) : Task<IDictionary<EasyIssueLabel, Issue array>> = task {
        let! issues =
            filters
            |> Seq.map (fun x -> getIssues client x)
            |> Task.WhenAll

        let set = Set<int>(issues |> Seq.collect ^ fun (x, y) -> y |> Seq.map ^ fun x -> x.Number)

        let res = 
            issues
            |> Seq.map ^ fun (x, y) -> 
                (x, y 
                    |> Seq.filter ^ fun z -> Set.contains z.Number set 
                    |> Array.ofSeq)
            |> dict

        return res
    }