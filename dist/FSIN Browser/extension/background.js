chrome.runtime.onMessage.addListener(function (message) {
  if (!message || message.type !== "fsin-exit-fullscreen") {
    return;
  }

  chrome.windows.getCurrent(function (windowInfo) {
    if (windowInfo && windowInfo.id) {
      chrome.windows.update(windowInfo.id, { state: "normal" });
    }
  });
});
