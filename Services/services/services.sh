task_protocols() {
        GO="" && [ "${1}" = "go" ] && GO="-c" && shift
        docker-compose run --rm generator ruby /generate.rb ${GO} ${*}
        [ ! "${GO}" = "-c" ] && echo "add 'go' to really write."
}
task_build() {
        docker build --no-cache -t services/node:latest .
}
. "`dirname ${0}`/../.task.sh" build ${*}
