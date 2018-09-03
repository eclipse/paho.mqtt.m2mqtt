dotnet build m2mqtt.sln --configuration=release

Remove-Item .\.nuget -Recurse -ErrorAction Ignore
New-Item -ItemType directory -Path .\.nuget

Remove-Item .\nupkg -Recurse -ErrorAction Ignore
New-Item -ItemType directory -Path .\nupkg

wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile .\.nuget\nuget.exe
.\.nuget\NuGet.exe pack M2Mqtt.nuspec -OutputDirectory ".\nupkg\"
