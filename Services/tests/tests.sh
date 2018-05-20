CWD=$(cd `dirname ${0}`; pwd)
TESTS=`find "${CWD}" -mindepth 1 -maxdepth 1 -type d | awk -F/ '{print $NF}'`
for t in ${TESTS}; do
        eval "task_${t}() { sh ./${t}/test.sh \${*}; }"
done
task_down() { for s in `ls ./*/test.sh`; do sh ${s} down; done; }
task_test() { task_down; for s in `ls ./*/test.sh`; do sh ${s} test; sh ${s} down; done; }
task_build() { task_down; for s in `ls ./*/test.sh`; do sh ${s} build; done; }
. "`dirname ${0}`/../.run/task.sh" ${*}
