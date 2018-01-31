CWD=$(cd `dirname ${0}`; pwd)
TASK="${1:-"generate"}"
shift
set -e
cd "${CWD}"

task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_generate() {
        docker-compose run --rm --no-deps generator ruby generate.rb
}
task_build() {
        task_down; docker-compose build --force-rm --pull
}
task_${TASK} ${*}
