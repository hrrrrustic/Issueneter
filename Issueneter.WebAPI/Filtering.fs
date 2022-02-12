module Filtering
    open System
    open IssueLabels
    open Octokit

    type Filter =
        | GithubIssueLabelFilter of IssueLabel
        | LocalIssueIgnoreLabelFilter of IssueLabel

    module Filter =
        let getLabelsForGithub (filter : Filter) : (IssueLabel Option) =
            match filter with
            | GithubIssueLabelFilter gf -> Some gf
            | LocalIssueIgnoreLabelFilter _ -> None

        let getLabelsForLocalIgnore (filter : Filter) : (IssueLabel Option) =
            match filter with
            | GithubIssueLabelFilter _ -> None
            | LocalIssueIgnoreLabelFilter i -> Some i

        let checkIgnoreFilters (filters : seq<Filter>) (issue : Issue) : bool =
            let ignoreLabels = filters |> Seq.map getLabelsForLocalIgnore |> Seq.choose id |> Seq.map IssueLabel.toString
            issue.Labels |> Seq.exists (fun l -> ignoreLabels |> Seq.contains l.Name) |> not


    type FilterConfiguration = {
        since : DateTimeOffset
        filters : seq<Filter>
    }

    let getDefaultFilters =
        [|
            GithubIssueLabelFilter ApiReadyForReview
            GithubIssueLabelFilter UpForGrabs
            GithubIssueLabelFilter Easy
            GithubIssueLabelFilter ApiApproved
            LocalIssueIgnoreLabelFilter InPr
        |]

    let getDefaultFilterConfiguration (since: DateTimeOffset) =
            {
                since = since
                filters = getDefaultFilters
            }