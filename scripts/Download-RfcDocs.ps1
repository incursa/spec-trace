param(
    [string]$OutputDirectory = ".\quic-rfcs",
    [ValidateSet("html", "txt", "both")]
    [string]$Format = "txt",
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$rfcs = @(
    @{ Number = 8999; Title = "Version-Independent Properties of QUIC" }
    @{ Number = 9000; Title = "QUIC: A UDP-Based Multiplexed and Secure Transport" }
    @{ Number = 9001; Title = "Using TLS to Secure QUIC" }
    @{ Number = 9002; Title = "QUIC Loss Detection and Congestion Control" }

    @{ Number = 9114; Title = "HTTP/3" }
    @{ Number = 9204; Title = "QPACK: Field Compression for HTTP/3" }

    @{ Number = 9221; Title = "An Unreliable Datagram Extension to QUIC" }
    @{ Number = 9287; Title = "Greasing the QUIC Bit" }
    @{ Number = 9308; Title = "Applicability of the QUIC Transport Protocol" }
    @{ Number = 9312; Title = "Manageability of the QUIC Transport Protocol" }
    @{ Number = 9368; Title = "Compatible Version Negotiation for QUIC" }
    @{ Number = 9369; Title = "QUIC Version 2" }

    # Closely related RFCs
    @{ Number = 9220; Title = "Bootstrapping WebSockets with HTTP/3" }
    @{ Number = 9250; Title = "DNS over Dedicated QUIC Connections" }
    @{ Number = 9297; Title = "HTTP Datagrams and the Capsule Protocol" }
    @{ Number = 9298; Title = "Proxying UDP in HTTP" }
    @{ Number = 9484; Title = "Proxying IP in HTTP" }
    @{ Number = 9461; Title = "Service Binding Mapping for DNS Servers" }
    @{ Number = 9463; Title = "DHCP and Router Advertisement Options for Encrypted DNS" }
    @{ Number = 9464; Title = "IKEv2 Configuration for Encrypted DNS" }
)

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$formatsToDownload = switch ($Format) {
    "html" { @("html") }
    "txt"  { @("txt") }
    "both" { @("html", "txt") }
}

$manifest = foreach ($rfc in $rfcs) {
    foreach ($extension in $formatsToDownload) {
        $number = $rfc.Number
        $fileName = "rfc$number.$extension"
        $targetPath = Join-Path $OutputDirectory $fileName
        $url = "https://www.rfc-editor.org/rfc/rfc$number.$extension"

        if ((Test-Path $targetPath) -and -not $Force) {
            Write-Host "Skipping RFC $number ($extension), already exists: $targetPath"
        }
        else {
            Write-Host "Downloading RFC $number ($extension): $($rfc.Title)"

            try {
                Invoke-WebRequest `
                    -Uri $url `
                    -OutFile $targetPath `
                    -MaximumRedirection 5 `
                    -ErrorAction Stop

                Start-Sleep -Milliseconds 250
            }
            catch {
                Write-Warning "Failed to download RFC $number from $url"
                Write-Warning $_.Exception.Message
                continue
            }
        }

        [pscustomobject]@{
            Number = $number
            Title  = $rfc.Title
            Format = $extension
            Url    = $url
            File   = $targetPath
        }
    }
}

$manifestPath = Join-Path $OutputDirectory "manifest.csv"
$manifest | Export-Csv -Path $manifestPath -NoTypeInformation

Write-Host ""
Write-Host "Done."
Write-Host "Files saved to: $OutputDirectory"
Write-Host "Manifest saved to: $manifestPath"