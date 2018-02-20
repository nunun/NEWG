task_services() { sh ./tests/services/test.sh ${*}; }
task_down() { for s in `ls ./tests/*/test.sh`; do sh ${s} down; done; }
task_test() { for s in `ls ./tests/*/test.sh`; do sh ${s} test; sh ${s} down; done; }
task_build() { for s in `ls ./tests/*/test.sh`; do sh ${s} build; done; }
. "`dirname ${0}`/../run.sh" task test ${*}
