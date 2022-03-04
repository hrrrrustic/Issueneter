module Github
    open System
    open System.Threading.Tasks
    open Filtering
    open Octokit
    open FSharp.Control.Tasks
    open IssueLabels

    let createRepositoryRequest (since : DateTimeOffset) (label : string) =
        let request = RepositoryIssueRequest(
                        Filter = IssueFilter.All,
                        State = ItemStateFilter.Open,
                        SortProperty = IssueSort.Updated,
                        SortDirection = SortDirection.Descending,
                        Since = since)
        request.Labels.Add label
        request

    let requestIssues (client: GitHubClient) (filterConfiguration: FilterConfiguration) = task {
        let! issuesArray =
            filterConfiguration.filters
            |> Seq.map Filter.getLabelsForGithub
            |> Seq.choose id
            |> Seq.map IssueLabel.toString
            |> Seq.map (createRepositoryRequest filterConfiguration.since)
            |> Seq.map (fun req -> client.Issue.GetAllForRepository("dotnet", "runtime", req))
            |> Task.WhenAll
        let issues = issuesArray |> Seq.concat |> Seq.distinctBy (fun i -> i.Id)
        return issues
    }

    let requestIssueEvents (client: GitHubClient) (issue: Issue) =
        client.Issue.Timeline.GetAllForIssue("dotnet", "runtime", issue.Number)

    let private credentionals = Credentials("token")
    let client =
        let value = GitHubClient(ProductHeaderValue("Issueneter"))
        value

    let getIssues = requestIssues client

    let getIssueEvents: Issue -> Task<Collections.Generic.IReadOnlyList<TimelineEventInfo>> = requestIssueEvents client