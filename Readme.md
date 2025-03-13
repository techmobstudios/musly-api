# Documentation

## Docker Image Deploy
`docker build -t rlshaw/musly-api .`

`docker build -t rlshaw/musly-api:latest -t rlshaw/musly-api:v2.1 .`


## Update Docker Image

### Copy source list into docker image
`docker cp sources.list 3d38aa158473:/etc/apt/`

### Install new version of .net core
`apt-get install -y dotnet-sdk-6.0`

`apt install -y aspnetcore-runtime-6.0`

` dotnet --version`

### Save Image

`docker commit 3d38aa158473 rlshaw/musly-dotnet6`

# Run Small test 
`docker run -it -p 2300:80 -v /Users/rickyshaw/repo/certified-mixtapes-core/CMTZ/wwwroot/UploadedFiles/Mixtapes:/home/music rlshaw/musly-api:1.0`

# Run Large Test
`docker run -it -p 2300:80 -v /Users/rickyshaw/Downloads/rd-imports:/home/music rlshaw/musly-api:1.0`


