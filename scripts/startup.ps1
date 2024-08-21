# Define the URL of the JDK package
$jdkUrl = "https://download.java.net/java/GA/jdk11/9/GPL/openjdk-11.0.2_windows-x64_bin.zip"


# Set the target installation path
$installPath = "C:\Users\Alfred.Stangl\repos\personal\FHIR.Starter\jdk"

# Create the installation directory if it does not exist
if (-Not (Test-Path -Path $installPath)) {
    New-Item -ItemType Directory -Force -Path $installPath
}

# Define the path for the downloaded file
$downloadedFile = "C:\Users\Alfred.Stangl\repos\personal\FHIR.Starter\jdk.zip"

# Download the JDK zip file
Invoke-WebRequest -Uri $jdkUrl -OutFile $downloadedFile

# Extract the JDK zip file
Expand-Archive -Path $downloadedFile -DestinationPath $installPath -Force

# # Set JAVA_HOME environment variable
# $env:JAVA_HOME = "$installPath\jdk-11.0.2"
#
# # Add Java to the PATH environment variable
# $env:Path = "$env:JAVA_HOME\bin;$env:Path"

# Verify Java installation
# java -version
