task_up() { task_down; docker-compose up; }
task_down() { docker-compose down; }
task_services() { sh ./services/services.sh ${*}; }
task_test() { sh ./test/test.sh ${*}; }
task_unity() {
        echo "building unity projects ..."
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseClientWebGL
        unity -batchmode -quit -executeMethod GameBuildMenuItems.BuildReleaseServerLinuxHeadless
}
task_build() {
        echo "building docker images ..."
        task_down
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        docker-compose build --force-rm
        docker-compose run --rm --no-deps matching npm install
        docker-compose run --rm --no-deps api      npm install
        docker-compose run --rm --no-deps mindlink npm install
}
task_clean() {
        echo "cleaning build caches ..."
        find ${PROJECT_TASK_DIR} -name "node_modules"      -exec rm -rf '{}' +
        find ${PROJECT_TASK_DIR} -name "package-lock.json" -exec rm -rf '{}' +
}
task_bundle() {
        echo "bundling compose files into a stack compose file ..."
        OUTPUT_FILENAME=".stack.${ENV_NAME}.yml"
        (cd ${PROJECT_TASK_DIR}; sh run.sh services build)
        BUNDLE_YAMLS="-f docker-compose.yml -f docker-compose.bundle.yml"
        CONFIG_YAMLS="-f docker-compose.yml -f docker-compose.stack.yml"
        docker-compose ${BUNDLE_YAMLS} build --force-rm --no-cache
        docker-compose ${BUNDLE_YAMLS} push
        docker-compose ${CONFIG_YAMLS} config --resolve-image-digest > ${OUTPUT_FILENAME}
        echo ""
        echo "successfully to bundle compose files into a stack compose file '${OUTPUT_FILENAME}'."
        echo ""
        echo "'docker stack deploy' to deploy services on host."
        echo "ex) docker stack deploy services --compose-file ${OUTPUT_FILENAME}"
        echo "    docker stack rm services"
}
. "`dirname ${0}`/.task.sh" up ${*}
