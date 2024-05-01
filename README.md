# TvMazeScraper
Scraper and API for tvmaze.com.
This solution consists of two applications:
1. A console application that queries show and cast data from api.tvmaze.com, and seeds a database with that data
2. A small api service that exposes the data in the database.

Both applications use two providers as an underlying database: a local file system based retrieval and a mongodb database.
The applications will use local file system by default, unless provided with a mongo db connection string as argument parameter (see [Running](#Running))

## Running
Both applications run dotnet 8. Make sure you have the sdk installed.

### Scraper
`cd` into the solution directory.

Run `dotnet run --project TvMaze.Scraper`

The scraper accepts some command line arguments, including a *verbose* flag for verbose logging and a *sample* flag for a short test run of ~250 shows.

The scraper runs on the local filesystem by default. To use mongo db as a database provider, provide a mongodb connection string, for example `--mongodb=mongo://localhost:27017/`

Run `dotnet run --project TvMaze.Scraper -- --help` for more information


### API
`cd` into the solution directory.

Run `dotnet run --project TvMaze.Api`

The API runs on the local filesystem by default. To use mongo db as a database provider, provide a mongodb connection string as a command line argumeent, for example `--mongodb=mongo://localhost:27017/`

The api will listen to port 5035 by default.
The api uses swagger. To see all available endpoints visit [http://localhost:5035/swagger/](http://localhost:5035/swagger/)

#### notes
- The fetch endpoints supprt ordering of the cast by birtyday. Acceptable values are "asc" for ascending order or "desc" for descending order
