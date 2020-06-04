# bbb_scaler

## Intro

The bbb_scaler scales big blue button intances in the ionos cloud using
metrics of node exporter, big blue button api and prometheus.

This project is involved as autoscaler in the [terraform setup] (https://github.com/schul-cloud/bbb-deployment) of schul cloud.

## Requirements

The target machines need metric interfaces of big blue button, node exporter and a registration in prometheus.
The node exporter metrics have to be exposed on port 9100.

This solution was built to use the ionos api v5.

## Scaling Behavior

1. Get all machines in the ionos data center
2. Filter all machines
  1. Get only big blue button instances
  2. Get only running instances
3. Retrieve workload
  1. Get CPU Utilization via Graphana and Prometheus
  2. Get Memory Utilization via Node Exporter
  3. Check if there are still active meetings
4. Start Scaling
  1. If the average workload of all machines is higher than the allowed cpu or memory utilization, start a new instance
  2. If the workload of an instance has the minimum cpu and memory settings and is still not utilized, shut it down
  3. If the memory workload of an instance is lower than the down scaling threshold MIN_ALLOWED_MEMORY_WORKLOAD and the memory is higher than DEFAULT_WORKER_MEMORY, decrease memory by 1024 bytes
  4. If the cpu workload of an instance is lower than the down scaling threshold MIN_ALLOWED_CPU_WORKLOAD and the cores are higher than DEFAULT_WORKER_MEMORY, decrease cores by 1 unit
  5. If the memory workload of an instance is higher than the scaling threshold MAX_ALLOWED_MEMORY_WORKLOAD and the memory is lower than or equal MAX_WORKER_MEMORY - 1024, increase memory by 1024 bytes
  6. If the cpu workload of an instance is higher than the scaling threshold MAX_ALLOWED_CPU_WORKLOAD and the cores + 1 are lower than or equal MAX_WORKER_CPU - 1, increase cores by 1 unit
5. Wait for WAITING_TIME miliseconds
  
## Settings
The autoscaler can be setup with environment variables.

Name | Default Value | Description
--- | --- | --- 
MINIMUM_ACTIVE_MACHINES | 2 | Amount of big blue button instances that should always active
MAX_ALLOWED_MEMORY_WORKLOAD | 0.35 | Maximum memory utilization until up scaling starts
MAX_ALLOWED_CPU_WORKLOAD | 0.35 | Maximum cpu utilization until up scaling starts
MIN_ALLOWED_MEMORY_WORKLOAD | 0.15 | Minimum memory utilization until down scaling starts
MIN_ALLOWED_CPU_WORKLOAD | 0.05 | Minimum cpu utilization until down scaling starts
MAX_WORKER_MEMORY | 16384 | Ceiling limit of memory scaling
DEFAULT_WORKER_MEMORY | 8192 | Floor limit of memory scaling
MAX_WORKER_CPU | 4 | Ceiling limit of cpu scaling
DEFAULT_WORKER_CPU | 2 | Floor limit of cpu scaling
WAITING_TIME | 300000 | Time in miliseconds until next scaling cyclus starts

Secrets can be also applied using environment variables

Name | Description
--- | ---
IONOS_USER | User name for ionos cloud
IONOS_PASS | Password for ionos cloud user
IONOS_DATACENTER | UUID of the data center in ionos where are the big blue button instaces located 
BBB_PASS | Key to access metric values of the big blue button instaces
GRAPHANA_PASS | API Key to call prometheus via graphana
NE_BASIC_AUTH_USER | Node exporter basic authentication user
NE_BASIC_AUTH_PASS | Node exporter basic authentication password

## Docker Build

Use the Dockerfile to build the application.
You don't need additional tools because the file already contains all build utils.

## Docker Compose File

Setup all secrets in the compose file using the [environment variable section] (https://docs.docker.com/compose/environment-variables/)
