task_protocols() {
        echo_info "generate protocol codes ..."
        GO="" && [ "${1}" = "go" ] && GO="-c" && shift
        docker-compose run --rm generator ruby /generate.rb ${GO} ${*}
        [ "${GO}" = "-c" ] && \
                echo_info "done." || \
                echo_info "add 'go' to really write."
}
task_build() {
        echo_info "build base image ..."
        docker build --no-cache -f ./Dockerfile.node.base -t services/node:base .
        echo_info "done."
}
. "`dirname ${0}`/../.run/task.sh" ${*}
