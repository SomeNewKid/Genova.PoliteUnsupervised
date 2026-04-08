# Genova.PoliteUnsupervised

An ML.NET-based .NET 8 solution for training and running an unsupervised classifier that labels input text as polite, rude, or neutral.

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

## License

GNU General Public License v3.0
