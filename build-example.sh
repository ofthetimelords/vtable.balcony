set -x
PL_VERSION=$(date '+%Y%m%d%H%M%S')

docker buildx build  --progress=plain --platform linux/amd64 -t <registry>/<yourtag>:${PL_VERSION} \
	-t <registry>/<yourtag>:latest --push --build-arg CACHEBUST=$(date +%s) .
