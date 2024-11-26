# Get the current directory where the script is located
$sourceDir = $PSScriptRoot

# Create timestamp-based output directory
$timestamp = Get-Date -Format "yy-MM-dd_HH_mm_ss"
$outputDir = "C:\temp\$timestamp"

# Create the output directory if it doesn't exist
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Find all .cs files in the current directory and subdirectories
$csFiles = Get-ChildItem -Path $sourceDir -Filter "*.cs" -Recurse

foreach ($file in $csFiles) {
    # Get the relative path from the source directory
    $relativePath = $file.FullName.Substring($sourceDir.Length + 1)
    
    # Split the path into directory and filename
    $directory = Split-Path $relativePath
    $filename = Split-Path $relativePath -Leaf
    
    # Create the new filename
    if ($directory) {
        # Replace directory separators and spaces with underscores
        $newFilename = ($directory -replace '\\', '_' -replace ' ', '_') + '_' + ($filename -replace ' ', '_')
    } else {
        # Just replace spaces with underscores in the filename
        $newFilename = $filename -replace ' ', '_'
    }
    
    # Construct the full output path
    $outputPath = Join-Path $outputDir $newFilename
    
    # Copy the file to the output location
    Copy-Item -Path $file.FullName -Destination $outputPath
    
    # Output the operation details
    Write-Host "Copied: $($file.FullName)"
    Write-Host "To: $outputPath"
    Write-Host "---"
}

Write-Host "Processing complete. Files copied to: $outputDir"
