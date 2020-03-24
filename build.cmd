dotnet restore
dotnet build
docker build -t acquiringbanksimulator -f AcquiringBank.Simulator/Dockerfile .
docker build -t paymentgatewayapi -f PaymentGateway.Api/Dockerfile .
docker build -t identityserver -f PaymentGateway.IdentityServer/Dockerfile .
::Create certificates
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p admin
dotnet dev-certs https --trust