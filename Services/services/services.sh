task_protocols() {
        docker-compose run --rm generator ruby generate.rb ${*}
}
task_build() {
        docker build -t newg/node:latest .
}
. "`dirname ${0}`/../.task.sh" build ${*}
