.\build.ps1

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3)

Nuget.exe push .\Nuget\SocketLite.PCL.$version.nupkg -Source https://www.nuget.org