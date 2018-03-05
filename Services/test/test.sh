CWD=$(cd `dirname ${0}`; pwd)
TESTS=`find "${CWD}/composes" -mindepth 1 -maxdepth 1 -type d | awk -F/ '{print $NF}'`
for t in ${TESTS}; do
        eval "task_${t}() { sh ./composes/${t}/test.sh \${*}; }"
done
task_down() { for s in `ls ./composes/*/test.sh`; do sh ${s} down; done; }
task_test() { task_down; for s in `ls ./composes/*/test.sh`; do sh ${s} test; sh ${s} down; done; }
task_build() { task_down; for s in `ls ./composes/*/test.sh`; do sh ${s} build; done; }
task_clean() { task_down; for s in `ls ./composes/*/test.sh`; do sh ${s} clean; done; }
. "`dirname ${0}`/../.task.sh" test ${*}
