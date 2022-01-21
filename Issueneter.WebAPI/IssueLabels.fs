module IssueLabels
    open Octokit

    type EasyIssueLabel =
    | ApiApproved
    | ApiReadyForReview
    | UpForGrabs
    | Easy

    type FoundIssue = {
        Issue: Issue
        SearchLabel: EasyIssueLabel
    }

    type SearchFilter = {
        BaseFilter: RepositoryIssueRequest
        Label: EasyIssueLabel
    }