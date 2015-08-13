var config = {
    version: '0.0.1',
    mode: 'production', //development or production
    serverApiUrl: 'http://139.129.15.91:8020/Api/',
    baseUrl: 'http://139.129.15.91:8020/',
    openId: getUserInfo().openid,
    nickName: getUserInfo().nickName,
    uploadStartDate: new Date(2015, 7, 11),
    voteStartDate: new Date(2015, 9, 1),
    showStartDate: new Date(2015, 9, 20),
    compareDateWithToday: function (date) {
        var today = new Date();
        if (date > today)
            return false;
        else
            return true;
    }
};

function getUserInfo() {
    var userInfo = window.localStorage.getItem("app-user-info");
    if (userInfo === null || userInfo === undefined) {
        userInfo = { openid: undefined, nickname: undefined };
        return userInfo;
    }

    return JSON.parse(userInfo);
}

angular.module("appConfig", [])
    .value('config', config);

