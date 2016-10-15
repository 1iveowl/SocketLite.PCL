./build
$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '.\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3)
$version = "NuGet\SocketLite.PCL." + $version + ".nupkg"
NuGet.exe push $version -Verbosity detailed -Source "http://packages.nuget.org/v1/FeedService.svc"
# NuGet.exe push $version -Verbosity detailed -Source "http://packages.nuget.org"