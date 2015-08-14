var config = {
    version: '0.0.1',
    mode: 'production', //development or production
    serverApiUrl: 'http://139.129.15.91:8020/Api/',
    baseUrl: 'http://139.129.15.91:8020/',
    openId: getUserInfo().openid,
    nickName: getUserInfo().nickname,
    uploadStartDate: new Date(2015, 7, 16),
    voteStartDate: new Date(2015, 8, 30),
    showStartDate: new Date(2015, 9, 19),
    compareDateWithToday: function (date) {
        var today = new Date();
        return date < today
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

