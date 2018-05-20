docker pull fu-n.net:5000/services/dotrun:latest
docker run -v `ospath "${PROJECT_TASK_DIR}/.run"`:/dotrun/run \
        fu-n.net:5000/services/dotrun:latest rsync -ahv --delete .run/* run
