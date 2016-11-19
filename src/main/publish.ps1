$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild ISocketLite.PCL\ISocketLite.PCL.csproj /t:Build /p:Configuration="Release"
&$msbuild SocketLite.PCL\SocketLite.PCL.csproj /t:Build /p:Configuration="Release"
&$msbuild Foundation\SocketLite.NET\SocketLite.NET.csproj /t:Build /p:Configuration="Release"
&$msbuild Foundation\SocketLite.WinRT\SocketLite.WinRT.csproj /t:Build /p:Configuration="Release"
&$msbuild Socket.CrossPlatform\SocketLite.PCL.Android\SocketLite.PCL.Android.csproj /t:Build /p:Configuration="Release"
&$msbuild Socket.CrossPlatform\SocketLite.PCL.iOS\SocketLite.PCL.iOS.csproj /t:Build /p:Configuration="Release"
&$msbuild Socket.CrossPlatform\SocketLite.PCL.UWP\SocketLite.PCL.UWP.csproj /t:Build /p:Configuration="Release"
$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '.\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3)
Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
NuGet.exe pack SocketLite.PCL.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version
Nuget.exe push .\Nuget\SocketLite.PCL.$version.nupkg -Source https://www.nuget.org