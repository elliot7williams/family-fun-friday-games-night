(function () {
  if (document.getElementById("fsin-hover-root")) {
    return;
  }

  var HOME_URL = "http://127.0.0.1:FSIN_PORT/index.html";

  function navigate(value) {
    var text = (value || "").trim();
    if (!text) {
      return;
    }

    if (/^(https?:\/\/|file:\/\/)/i.test(text)) {
      window.location.href = text;
      return;
    }

    if (/^[\w.-]+\.[a-z]{2,}(?::\d+)?(?:\/.*)?$/i.test(text)) {
      window.location.href = "https://" + text;
      return;
    }

    window.location.href = "https://www.bing.com/search?q=" + encodeURIComponent(text);
  }

  var style = document.createElement("style");
  style.textContent = [
    "#fsin-hover-root{all:initial;position:fixed;inset:0;z-index:2147483640;pointer-events:none;font-family:Arial,Helvetica,sans-serif;}",
    "#fsin-left-zone{position:fixed;left:0;top:0;width:104px;height:104px;pointer-events:auto;background:transparent;}",
    "#fsin-right-zone{position:fixed;right:0;top:0;width:104px;height:100vh;pointer-events:auto;background:transparent;}",
    "#fsin-top-zone{position:fixed;left:104px;right:104px;top:0;height:70px;pointer-events:auto;background:transparent;}",
    "#fsin-controls{position:fixed;left:14px;top:14px;display:flex;gap:8px;opacity:0;transform:translateY(-8px);transition:opacity .16s ease,transform .16s ease;pointer-events:none;}",
    "#fsin-left-zone:hover~#fsin-controls,#fsin-controls:hover,#fsin-controls.is-visible{opacity:1;transform:translateY(0);pointer-events:auto;}",
    "#fsin-fullscreen-controls{position:fixed;right:14px;top:50%;display:flex;opacity:0;transform:translate(8px,-50%);transition:opacity .16s ease,transform .16s ease;pointer-events:none;}",
    "#fsin-right-zone:hover~#fsin-fullscreen-controls,#fsin-fullscreen-controls:hover{opacity:1;transform:translate(0,-50%);pointer-events:auto;}",
    ".fsin-button{width:44px;height:44px;border:0;border-radius:50%;box-sizing:border-box;background:rgba(8,15,24,.84);color:#fff;font:700 24px/44px Arial,Helvetica,sans-serif;text-align:center;box-shadow:0 8px 24px rgba(0,0,0,.28),inset 0 0 0 1px rgba(255,255,255,.22);cursor:pointer;padding:0;margin:0;appearance:none;}",
    ".fsin-wide-button{width:auto;min-width:116px;border-radius:8px;padding:0 14px;font-size:14px;}",
    ".fsin-button:active{transform:translateY(1px) scale(.97);}",
    "#fsin-url-panel{position:fixed;left:50%;top:12px;width:min(820px,calc(100vw - 260px));height:48px;display:grid;grid-template-columns:1fr 72px;opacity:0;transform:translate(-50%,-10px);transition:opacity .16s ease,transform .16s ease;pointer-events:none;border-radius:8px;overflow:hidden;background:rgba(8,15,24,.88);box-shadow:0 12px 32px rgba(0,0,0,.32),inset 0 0 0 1px rgba(255,255,255,.18);}",
    "#fsin-top-zone:hover~#fsin-url-panel,#fsin-url-panel:hover,#fsin-url-panel:focus-within{opacity:1;transform:translate(-50%,0);pointer-events:auto;}",
    "#fsin-url-input{min-width:0;border:0;outline:0;padding:0 16px;color:#fff;background:transparent;font:16px/48px Arial,Helvetica,sans-serif;}",
    "#fsin-url-go{border:0;color:#061018;background:#7de4ff;font:700 14px/48px Arial,Helvetica,sans-serif;cursor:pointer;padding:0;margin:0;}",
    "@media(max-width:700px){#fsin-url-panel{left:14px;right:14px;width:auto;transform:translateY(-10px);}#fsin-top-zone:hover~#fsin-url-panel,#fsin-url-panel:hover,#fsin-url-panel:focus-within{transform:translateY(0);}}"
  ].join("");

  var root = document.createElement("div");
  root.id = "fsin-hover-root";
  root.innerHTML = [
    '<div id="fsin-left-zone"></div>',
    '<div id="fsin-right-zone"></div>',
    '<div id="fsin-top-zone"></div>',
    '<div id="fsin-controls">',
    '<button class="fsin-button" id="fsin-back" type="button" title="Back" aria-label="Back">&#8249;</button>',
    '<button class="fsin-button" id="fsin-forward" type="button" title="Forward" aria-label="Forward">&#8250;</button>',
    '<button class="fsin-button" id="fsin-refresh" type="button" title="Refresh" aria-label="Refresh">&#8635;</button>',
    '<button class="fsin-button" id="fsin-home" type="button" title="Home" aria-label="Home">&#8962;</button>',
    '</div>',
    '<div id="fsin-fullscreen-controls">',
    '<button class="fsin-button fsin-wide-button" id="fsin-exit-fullscreen" type="button" title="Exit full screen" aria-label="Exit full screen">Exit Full</button>',
    '</div>',
    '<form id="fsin-url-panel" autocomplete="off">',
    '<input id="fsin-url-input" type="text" spellcheck="false" aria-label="Search or enter a website">',
    '<button id="fsin-url-go" type="submit">Go</button>',
    '</form>'
  ].join("");

  (document.head || document.documentElement).appendChild(style);
  (document.body || document.documentElement).appendChild(root);

  var controls = root.querySelector("#fsin-controls");
  var input = root.querySelector("#fsin-url-input");
  var form = root.querySelector("#fsin-url-panel");

  root.querySelector("#fsin-back").addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();
    window.history.back();
  }, true);

  root.querySelector("#fsin-forward").addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();
    window.history.forward();
  }, true);

  root.querySelector("#fsin-refresh").addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();
    window.location.reload();
  }, true);

  root.querySelector("#fsin-home").addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();
    window.location.href = HOME_URL;
  }, true);

  root.querySelector("#fsin-exit-fullscreen").addEventListener("click", function (event) {
    event.preventDefault();
    event.stopPropagation();

    if (document.fullscreenElement && document.exitFullscreen) {
      document.exitFullscreen();
    }

    try {
      chrome.runtime.sendMessage({ type: "fsin-exit-fullscreen" });
    } catch (error) {
      window.alert("Press F11 to exit full screen.");
    }
  }, true);

  form.addEventListener("submit", function (event) {
    event.preventDefault();
    event.stopPropagation();
    navigate(input.value);
  }, true);

  root.querySelector("#fsin-top-zone").addEventListener("mouseenter", function () {
    input.value = window.location.href;
    window.setTimeout(function () {
      input.focus();
      input.select();
    }, 60);
  });

  document.addEventListener("touchstart", function (event) {
    if (!event.touches || !event.touches.length) {
      return;
    }

    var touch = event.touches[0];
    if (touch.clientX <= 104 && touch.clientY <= 104) {
      controls.className = "is-visible";
      window.clearTimeout(root._fsinTimer);
      root._fsinTimer = window.setTimeout(function () {
        controls.className = "";
      }, 2600);
    }
  }, true);
}());
