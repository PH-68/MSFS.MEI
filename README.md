# MSFS.MEI(MSFS Map Enhancement Improved)
A lightweight application to replace original MSFS Bing maps with Google maps.

Memory usage is around 70MB

However this is still in very early development. Some feature might be missing. Such as cache, other map providers, High LOD...(Will implement in near future!)

## Usage
### Installing the trusted root certificate

Install the msfs2020-server-cert.pfx certificate in this repo to **trusted root certification authorities store**.

### Hostname resolution

append the following line to C:\Windows\System32\drivers\etc

```
127.0.0.1 kh.ssl.ak.tiles.virtualearth.net
127.0.0.1 khstorelive.azureedge.net
```

⚠️Remove this is you want original MSFS Bing maps

### Run the app

Run MSFS.MEI.exe

⚠️Make sure you got latest .NET 7 runtime installed

