SET configuration=Release
SET out=C:\Publish\Packages
SET spec=MongoProxy.nuspec

call dotnet pack -c %configuration% -p:NuspecFile=%spec% -o "%out%/EdisonTalk.MongoProxy"