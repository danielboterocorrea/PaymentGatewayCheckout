dotnet restore
dotnet build
docker build -t acquiringbanksimulator -f AcquiringBank.Simulator/Dockerfile .
docker build -t paymentgatewayapi -f PaymentGateway.Api/Dockerfile .
docker build -t identityserver -f PaymentGateway.IdentityServer/Dockerfile .
