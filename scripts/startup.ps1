# Define the URL of the JDK package
$jdkUrl = "https://download.java.net/java/GA/jdk11/9/GPL/openjdk-11.0.2_windows-x64_bin.zip"

# Set the target installation path
$installPath = "jdk"

# Create the installation directory if it does not exist
if (-Not (Test-Path -Path $installPath)) {
    New-Item -ItemType Directory -Force -Path $installPath
}

# Define the path for the downloaded file
$downloadedFile = "jdk.zip"

# Download the JDK zip file
Invoke-WebRequest -Uri $jdkUrl -OutFile $downloadedFile

# Extract the JDK zip file
Expand-Archive -Path $downloadedFile -DestinationPath $installPath -Force
