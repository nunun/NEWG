task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_build() {
        echo_info "build docker images ..."
        task_down
        (cd ${RUN_ROOT_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps matching npm install
        docker-compose run --rm --no-deps api      npm install
        docker-compose run --rm --no-deps mindlink npm install
        echo_info "done."
}
task_build_game() {
        echo_info "build game ..."
        unity -batchmode -quit -executeMethod GameBuilder.Build -gameSettingsName ${ENV_BUILD_GAME_CLIENT}
        unity -batchmode -quit -executeMethod GameBuilder.Build -gameSettingsName ${ENV_BUILD_GAME_SERVER}
        echo_info "done."
}
task_build_stack_file() {
        echo_info "build stack file ..."
        local stack_file="${1}"
        local build_yamls="-f docker-compose.yml -f docker-compose.stack.build.yml"
        local config_yamls="-f docker-compose.yml -f docker-compose.stack.deploy.yml"
        sh ./services/services.sh build
        #task_build_unity_project
        echo_info "config stack file ..."
        docker-compose ${build_yamls} build --force-rm --no-cache
        docker-compose ${build_yamls} push
        docker-compose ${config_yamls} config --resolve-image-digest > "${stack_file}"
        echo_info "done."
}
task_clean() {
        echo_info "clean build caches ..."
        find ${RUN_ROOT_DIR} -name "node_modules"      -exec rm -rf '{}' +
        find ${RUN_ROOT_DIR} -name "package-lock.json" -exec rm -rf '{}' +
        echo_info "done."
}
task_services() { sh ./services/services.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_stack() { sh ./.run/stack.sh ${*}; }
. "`dirname ${0}`/.run/task.sh" ${*}
