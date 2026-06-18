(function () {
  if (document.getElementById("games-night-back-zone")) {
    return;
  }

  var style = document.createElement("style");
  style.textContent = [
    "#games-night-back-zone{position:fixed;left:0;top:0;width:92px;height:92px;z-index:2147483646;background:transparent!important;}",
    "#games-night-back-button{position:fixed;left:14px;top:14px;z-index:2147483647;width:44px;height:44px;box-sizing:border-box;border:0;border-radius:50%;background:rgba(8,15,24,.82);color:#fff;font:28px/44px Arial,Helvetica,sans-serif;text-align:center;box-shadow:0 8px 24px rgba(0,0,0,.28),inset 0 0 0 1px rgba(255,255,255,.22);cursor:pointer;opacity:0;transform:translateY(-8px);transition:opacity .18s ease,transform .18s ease;pointer-events:none;-webkit-tap-highlight-color:transparent;padding:0;margin:0;appearance:none;}",
    "#games-night-back-zone:hover+#games-night-back-button,#games-night-back-button:hover,#games-night-back-button.is-visible{opacity:1;transform:translateY(0);pointer-events:auto;}",
    "#games-night-back-button:active{transform:translateY(1px) scale(.97);}"
  ].join("");

  var zone = document.createElement("div");
  var button = document.createElement("button");
  var hideTimer = 0;

  zone.id = "games-night-back-zone";
  button.id = "games-night-back-button";
  button.type = "button";
  button.innerHTML = "&#8249;";
  button.setAttribute("aria-label", "Go back");
  button.title = "Back";

  button.addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();
    window.history.back();
  }, true);

  document.addEventListener("touchstart", function (event) {
    if (event.touches && event.touches.length) {
      var touch = event.touches[0];
      if (touch.clientX <= 92 && touch.clientY <= 92) {
        button.className = "is-visible";
        window.clearTimeout(hideTimer);
        hideTimer = window.setTimeout(function () {
          button.className = "";
        }, 2600);
      }
    }
  }, true);

  (document.head || document.documentElement).appendChild(style);
  (document.body || document.documentElement).appendChild(zone);
  (document.body || document.documentElement).appendChild(button);
}());
