(function () {
  function addAppControls() {
    if (document.getElementById("games-night-back-button")) {
      return;
    }

    var style = document.createElement("style");
    style.textContent = [
      "#games-night-back-zone{position:fixed;left:0;top:0;width:92px;height:92px;z-index:2147483646;}",
      "#games-night-back-button{position:fixed;left:14px;top:14px;z-index:2147483647;width:44px;height:44px;border:0;border-radius:50%;background:rgba(8,15,24,.82);color:#fff;font:28px/44px Arial,Helvetica,sans-serif;text-align:center;box-shadow:0 8px 24px rgba(0,0,0,.28),inset 0 0 0 1px rgba(255,255,255,.22);cursor:pointer;opacity:0;transform:translateY(-8px);transition:opacity .18s ease,transform .18s ease;pointer-events:none;-webkit-tap-highlight-color:transparent;}",
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

      if (window.history.length > 1) {
        window.history.back();
      } else {
        window.location.href = "index.html";
      }
    });

    document.addEventListener("touchstart", function () {
      button.className = "is-visible";
      window.clearTimeout(hideTimer);
      hideTimer = window.setTimeout(function () {
        button.className = "";
      }, 2600);
    }, true);

    document.head.appendChild(style);
    document.body.appendChild(zone);
    document.body.appendChild(button);
  }

  function sameAppUrl(url) {
    try {
      return new URL(url, window.location.href).href;
    } catch (error) {
      return url;
    }
  }

  function keepLinkInApp(event) {
    var link = event.target;

    while (link && link.tagName !== "A") {
      link = link.parentNode;
    }

    if (!link || !link.href || link.href.indexOf("javascript:") === 0 || link.href.indexOf("mailto:") === 0) {
      return;
    }

    if (link.target && link.target !== "_self") {
      event.preventDefault();
      window.location.href = sameAppUrl(link.href);
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", addAppControls);
  } else {
    addAppControls();
  }

  document.addEventListener("click", keepLinkInApp, true);

  window.open = function (url) {
    if (url) {
      window.location.href = sameAppUrl(url);
    }

    return window;
  };

  Array.prototype.forEach.call(document.querySelectorAll("a[target], form[target]"), function (element) {
    element.setAttribute("target", "_self");
  });
}());
