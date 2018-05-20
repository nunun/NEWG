task_update() {
        local deploy_tag=`deploy_tag ${*}`
        echo "update current .run by '${deploy_tag}' ..."
        cd ${PROJECT_TASK_DIR}
        docker pull ${deploy_tag}
        docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
                ${deploy_tag} rsync -ahv --delete .run/* run
}

task_diff() {
        local deploy_tag=`deploy_tag ${*}`
        echo "diff between current .run and '${deploy_tag}' ..."
        cd ${PROJECT_TASK_DIR}
        docker pull ${deploy_tag}
        docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
                ${deploy_tag} diff -r .run run
}

task_push() {
        local deploy_tag=`deploy_tag ${*}`
        local bundle_dir="/tmp/dotrun"
        local dockerfile_path="${bundle_dir}/Dockerfile"
        echo "push current .run to '${deploy_tag}' ..."
        rm -rf "${bundle_dir}"
        mkdir -p "${bundle_dir}"
        cp -r "${PROJECT_TASK_DIR}/.run" "${bundle_dir}/.run"
        echo "FROM alpine"                   > "${dockerfile_path}"
        echo "RUN apk add --no-cache rsync" >> "${dockerfile_path}"
        echo "WORKDIR /dotrun"              >> "${dockerfile_path}"
        echo "ADD .run ./.run"              >> "${dockerfile_path}"
        (cd ${bundle_dir}; \
                docker build --no-cache -t "${deploy_tag}" .; \
                docker push "${deploy_tag}")
        echo "done."
}

deploy_tag() {
        echo "fu-n.net:5000/services/dotrun:${1:-"lastet"}"
}

. "`dirname ${0}`/task.sh" help ${*}
