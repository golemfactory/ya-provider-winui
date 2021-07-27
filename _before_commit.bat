REM Run this script before commit to fix formatting
REM Non-formatted code won't compile in CI builds
REM You can install dotnet-format using command: dotnet tool install -g dotnet-format
dotnet-format GolemUI.sln
PAUSE