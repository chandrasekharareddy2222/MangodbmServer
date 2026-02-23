[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$filePath = "c:\Users\devar\code\MangodbmServer\mara_data.csv"
$uri = "https://localhost:5001/api/v1/field-metadata/import-csv"

$boundary = [System.Guid]::NewGuid().ToString()
$fileBytes = [System.IO.File]::ReadAllBytes($filePath)

# Remove BOM if present (UTF-8 BOM is EF BB BF)
if ($fileBytes.Length -gt 3 -and $fileBytes[0] -eq 0xEF -and $fileBytes[1] -eq 0xBB -and $fileBytes[2] -eq 0xBF) {
    $fileBytes = $fileBytes[3..($fileBytes.Length - 1)]
}

$encoding = [System.Text.Encoding]::UTF8
$CRLF = "`r`n"

$bodyStart = "--{0}{1}Content-Disposition: form-data; name=`"file`"; filename=`"{2}`"{1}Content-Type: text/csv{1}{1}"
$beforeBytes = $encoding.GetBytes(($bodyStart -f $boundary, $CRLF, (Split-Path $filePath -Leaf)))
$bodyEnd = "{0}--{1}--{0}"
$afterBytes = $encoding.GetBytes(($bodyEnd -f $CRLF, $boundary))

$bodyStream = [System.IO.MemoryStream]::new()
$bodyStream.Write($beforeBytes, 0, $beforeBytes.Length)
$bodyStream.Write($fileBytes, 0, $fileBytes.Length)
$bodyStream.Write($afterBytes, 0, $afterBytes.Length)
$bodyStream.Position = 0
$body = $bodyStream.ToArray()
$bodyStream.Close()

try {
    $response = Invoke-WebRequest -Uri $uri -Method Post -ContentType ("multipart/form-data; boundary={0}" -f $boundary) -Body $body -UseBasicParsing -TimeoutSec 30
    Write-Host "Status Code: $($response.StatusCode)"
    Write-Host "Response:"
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error: $_"
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        Write-Host "Response Content: $($reader.ReadToEnd())"
        $reader.Close()
    }
}
