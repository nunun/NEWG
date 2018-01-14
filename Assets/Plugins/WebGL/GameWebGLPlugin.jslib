var GameWebGLPlugin = {
    _GetLocationURL: function() {
        var location = window.location.href;
        console.log(location);
        var size = lengthBytesUTF8(location) + 1;
        var ptr  = _malloc(size);
        stringToUTF8(location, ptr, size);
        return ptr;
    },
};
mergeInto(LibraryManager.library, GameWebGLPlugin);
