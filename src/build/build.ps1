$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild ..\main\ISocketLite.PCL\ISocketLite.PCL.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\SocketLite.PCL\SocketLite.PCL.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\Foundation\SocketLite.NET\SocketLite.NET.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\Foundation\SocketLite.Universal\SocketLite.Universal.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\Socket.CrossPlatform\SocketLite.PCL.Android\SocketLite.PCL.Android.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\Socket.CrossPlatform\SocketLite.PCL.iOS\SocketLite.PCL.iOS.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\Socket.CrossPlatform\SocketLite.PCL.UWP\SocketLite.PCL.UWP.csproj /t:Build /p:Configuration="Release"
$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3)


Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
NuGet.exe pack SocketLite.PCL.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version