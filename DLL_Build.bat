@echo off
dotnet msbuild ./Target/Target.csproj /p:OutputType=Library /p:DocumentationFile="Target.xml" /p:UseResultsCache="false"
dotnet msbuild ./LDocBuilder.csproj /p:OutputType=Library /p:DocumentationFile="LDocBuilder.xml" /p:UseResultsCache="false"
pause