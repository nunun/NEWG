task_build() {
        docker build -t newg/node:latest .
}
. "`dirname ${0}`/../.task.sh" build ${*}
