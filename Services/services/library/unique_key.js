var util          = require('util');
var UniqueKeyData = require('../protocols/models').UniqueKeyData;

// 固有キー
// モデル UniqueKeyData を使って固有キーの生成と破棄を行います。
// 固有キーを使い終わった後は、なるべく破棄を行って下さい。
// 破棄はし損ねてもただちに問題ありませんが
// 生成済みのキーがデータベースに残るため、遠い将来ディスクを圧迫します。
class UniqueKey {
    //------------------------------------------------------------------------- 固有キーの生成と破棄
    // 固有キーの生成
    static create(key, callback) {
        var uniqueKeyData = new UniqueKeyData();
        uniqueKeyData.save(key, (err, id, rev) => {
            if (callback) {
                callback(err, id);
            }
        });
    }

    // 固有キーの破棄
    static destroy(key, callback) {
        UniqueKeyData.get(key, (err, data) => {
            if (err) {
                if (callback) {
                    callback(err);
                }
                return;
            }
            UniqueKeyData.destroy(data._id, data._rev, (err) => {
                if (err) {
                    if (callback) {
                        callback(err);
                    }
                    return;
                }
                if (callback) {
                    callback(null);
                }
            });
        });
    }
}

module.exports = UniqueKey;
