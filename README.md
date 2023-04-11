# MSFS.MEI(MSFS Map Enhancement Improved)
A lightweight application to replace original MSFS Bing maps with Google maps.

This is still in very early development. Some feature might be missing. Such as cache, other map providers...(Will implement in near future!)

## Feature

- Original MSFS Bing maps replacement
- Lightweight(Memory is around 70MB)
- No ads/telemetry

## Usage

### Download

Download the latest artifacts from [Actions tab](https://github.com/PH-68/MSFS.MEI/actions)

### Installing the trusted root certificate

Install the msfs2020-server-cert.pfx certificate in this repo to **trusted root certification authorities store**.

### Hostname resolution

Append the following line to C:\Windows\System32\drivers\etc

```
127.0.0.1 kh.ssl.ak.tiles.virtualearth.net
127.0.0.1 khstorelive.azureedge.net
```

⚠️Remove this is you want original MSFS Bing maps

### Run the app

Run MSFS.MEI.exe

⚠️Make sure you got latest .NET 7 runtime installed if you're using no-self-contained version

## Settings
Refer to [wiki](https://github.com/PH-68/MSFS.MEI/wiki) for more
