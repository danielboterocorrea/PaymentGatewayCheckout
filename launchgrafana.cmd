docker run --rm -d -p 3000:3000 --name grafana grafana/grafana:6.5.0
docker exec -ti 6f51a0815b48 grafana-cli admin reset-admin-password admin