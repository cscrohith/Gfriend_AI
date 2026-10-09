$filePath = "GFriendUI\MainForm.Designer.cs"
$content = Get-Content $filePath -Raw

# Find the section with comboBoxTargetDevice
if ($content -match "comboBoxTargetDevice") {
	Write-Host "✓ Found comboBoxTargetDevice in the file"

	# Extract the relevant section
	$startIdx = $content.IndexOf("comboBoxTargetDevice")
	$section = $content.Substring([Math]::Max(0, $startIdx - 500), 1500)
	Write-Host "Context around comboBoxTargetDevice:"
	Write-Host $section
} else {
	Write-Host "✗ comboBoxTargetDevice NOT found"
}

# Check for labelTargetDevice
if ($content -match "labelTargetDevice") {
	Write-Host "`n✓ Found labelTargetDevice"
} else {
	Write-Host "`n✗ labelTargetDevice NOT found"
}

# Show file size
$file = Get-Item $filePath
Write-Host "`nFile size: $($file.Length) bytes"
