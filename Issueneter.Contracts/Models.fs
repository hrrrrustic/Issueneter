module Issueneter.Contracts.Models

open Issueneter.Contracts.Identifiers

type RepositoryInfo = {
    Url: RepositoryUrl
    Owner: RepositoryOwner
    Name: RepositoryName
}