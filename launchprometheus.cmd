SET var=%cd%
docker run --rm -d --name prometheus -p 9090:9090 -v "%var%/Configuration/prometheus.yml":"/etc/prometheus/prometheus.yml" prom/prometheus --config.file="/etc/prometheus/prometheus.yml"