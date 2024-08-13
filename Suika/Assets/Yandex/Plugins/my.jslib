mergeInto(LibraryManager.library, {

  Hello: function () {
    window.alert("Hello, world!");
    console.log("Hello world!");
  },
    GetPlayerDataExtern: function () {
    myGameInstance.SendMessage('Yandex', 'SetPlayerName', player.getName());
    myGameInstance.SendMessage('Yandex', 'SetPlayerPhoto', player.getPhoto("medium"));
  },
    RateGameExtern: function () {
    ysdk.feedback.canReview()
        .then(({ value, reason }) => {
            if (value) {
                ysdk.feedback.requestReview()
                    .then(({ feedbackSent }) => {
                        console.log(feedbackSent);
                    })
            } else {
                console.log(reason)
            }
        })
  },
  SaveExtern: function (data) {
    var dataString = UTF8ToString(data);
    var myobj = JSON.parse(dataString);
    player.setData(myobj);
  },
  LoadExtern: function () {
    if(player){
      player.getData().then(_data => {
        const myJSON = JSON.stringify(_data);
        myGameInstance.SendMessage('Progress', 'LoadData', myJSON);
        console.log("Data loaded");
      });
    }
  },
  GetLangExtern: function () {
    var lang = ysdk.environment.i18n.lang;
    var bufferSize = lengthBytesUTF8(lang) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(lang, buffer, bufferSize);
    return buffer;
  },
  ShowAdsExtern: function () {
    ysdk.adv.showFullscreenAdv({
    callbacks: {
        onClose: function(wasShown) {
          // Действие после закрытия рекламы.
        },
        onError: function(error) {
          // Действие в случае ошибки.
        }
      }
    })
  },
  ShowRewardedAdsExtern: function (value) {
    ysdk.adv.showRewardedVideo({
    callbacks: {
        onOpen: () => {
          console.log('Video ad open.');
        },
        onRewarded: () => {
          console.log('Rewarded!');
          myGameInstance.SendMessage('Progress', 'AddReward', value);
        },
        onClose: () => {
          console.log('Video ad closed.');
        }, 
        onError: (e) => {
          console.log('Error while open video ad:', e);
        }
    }
})
  },
});