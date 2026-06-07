# Genova.PoliteUnsupervised

An ML.NET-based .NET 8 solution for training and running an unsupervised classifier that labels input text as polite, rude, or neutral.

> [!WARNING]
> This is an experimental project and should not be considered production-ready. It exists to explore a small AI, ML, agent, or demo idea within the broader Genova ecosystem.

> [!IMPORTANT]
> A fresh public clone of this repository should not be expected to restore or build without additional Genova infrastructure. Many Genova dependencies are distributed through a private authenticated NuGet feed, and the public source does not include feed credentials or a complete public package graph.

## Installation

```bash
dotnet restore
dotnet build
```

## Usage

Run the console classifier:

```bash
dotnet run --project PoliteUnsupervised.Console
```

Type a sentence and press Enter.

Train the model artifacts:

```bash
dotnet run --project PoliteUnsupervised.Training
```

## Features

* Class library for text tone classification
* Console app for interactive sentence classification
* Training app that builds model and cluster-map artifacts from a dataset
* Returns label, cluster, distance, threshold, and confidence

## Notes

* Targets .NET 8.
* The training project expects `polite-rude-dataset.txt`.
* Training paths are configured in `PoliteUnsupervised.Training/appsettings.json`.
* The runtime library uses embedded model files from `PoliteUnsupervised/Data`.

## Thanks

* ML.NET
* The included `polite-rude-dataset.txt` dataset

## Third-Party Notices

This project has direct runtime dependencies on third-party NuGet packages, including `Microsoft.Extensions.*` packages (MIT), `Microsoft.ML*` packages (MIT). See each package's NuGet license metadata for full license and notice terms.

## License

GNU General Public License v3.0. See the `LICENSE` file for details.
