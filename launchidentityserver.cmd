docker run --rm -it -p 5003:80 -p 5002:443 -e ASPNETCORE_ENVIRONMENT="Production" -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=5002 -e ASPNETCORE_Kestrel__Certificates__Default__Password="admin" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ identityserver  --net=dockernet