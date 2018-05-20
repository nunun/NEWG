task_protocols() {
        echo "generate protocol codes ..."
        GO="" && [ "${1}" = "go" ] && GO="-c" && shift
        docker-compose run --rm generator ruby /generate.rb ${GO} ${*}
        [ "${GO}" = "-c" ] && echo "done." || echo "add 'go' to really write."
}
task_build() {
        echo "build base image ..."
        docker build --no-cache -f ./Dockerfile.node.base -t services/node:base .
        echo "done."
}
. "`dirname ${0}`/../.run/task.sh" ${*}
