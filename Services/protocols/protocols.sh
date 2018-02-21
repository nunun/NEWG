task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_go() {
        task_generate go
}
task_generate() {
        GO="" && [ "${1}" = "go" ] && GO="-c"
        docker-compose run --rm --no-deps generator ruby generate.rb ${GO}
        if [ "${GO}" = "" ]; then
                echo ""
                echo "add 'go' to really write."
        fi
}
task_build() {
        task_down; docker-compose build --force-rm --pull
}
. "`dirname ${0}`/../.task.sh" generate ${*}
