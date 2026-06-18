(function () {
  function addBubblePopStyles() {
    if (document.getElementById("fffg-bubble-pop-styles")) {
      return;
    }

    var style = document.createElement("style");
    style.id = "fffg-bubble-pop-styles";
    style.textContent = [
      ".bubble{cursor:pointer;-webkit-tap-highlight-color:transparent;}",
      ".bubble.fffg-bubble-popping{animation:fffgBubblePop 260ms ease-out forwards!important;transform-origin:center center!important;}",
      ".bubble.fffg-bubble-popping:after{height:calc(100% - 20px)!important;width:calc(100% - 20px)!important;}",
      ".bubble.fffg-bubble-popped{opacity:0!important;pointer-events:none!important;}",
      "@keyframes fffgBubblePop{0%{opacity:.65;transform:scale(1);}45%{opacity:.9;transform:scale(1.25);}100%{opacity:0;transform:scale(.15);}}"
    ].join("");
    document.head.appendChild(style);
  }

  function findBubbleAt(x, y) {
    var bubbles = document.querySelectorAll("#background-wrap .bubble:not(.fffg-bubble-popped):not(.fffg-bubble-popping)");

    for (var i = bubbles.length - 1; i >= 0; i--) {
      var bubble = bubbles[i];
      var rect = bubble.getBoundingClientRect();
      var radiusX = rect.width / 2;
      var radiusY = rect.height / 2;
      var centerX = rect.left + radiusX;
      var centerY = rect.top + radiusY;
      var normalizedX = (x - centerX) / radiusX;
      var normalizedY = (y - centerY) / radiusY;

      if ((normalizedX * normalizedX) + (normalizedY * normalizedY) <= 1) {
        return bubble;
      }
    }

    return null;
  }

  function playBubblePopSound() {
    var AudioContext = window.AudioContext || window.webkitAudioContext;

    if (!AudioContext) {
      return;
    }

    var audioContext = new AudioContext();
    var now = audioContext.currentTime;
    var oscillator = audioContext.createOscillator();
    var gain = audioContext.createGain();
    var filter = audioContext.createBiquadFilter();

    oscillator.type = "sine";
    oscillator.frequency.setValueAtTime(760, now);
    oscillator.frequency.exponentialRampToValueAtTime(180, now + 0.09);

    filter.type = "highpass";
    filter.frequency.setValueAtTime(120, now);

    gain.gain.setValueAtTime(0.001, now);
    gain.gain.exponentialRampToValueAtTime(0.18, now + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

    oscillator.connect(filter);
    filter.connect(gain);
    gain.connect(audioContext.destination);

    oscillator.start(now);
    oscillator.stop(now + 0.13);

    window.setTimeout(function () {
      audioContext.close();
    }, 180);
  }

  function popBubble(bubble) {
    var rect = bubble.getBoundingClientRect();

    playBubblePopSound();
    bubble.style.position = "fixed";
    bubble.style.left = rect.left + "px";
    bubble.style.top = rect.top + "px";
    bubble.style.width = rect.width + "px";
    bubble.style.height = rect.height + "px";
    bubble.style.margin = "0";
    bubble.style.transform = "none";
    bubble.className += " fffg-bubble-popping";

    window.setTimeout(function () {
      bubble.className = bubble.className.replace(/\bfffg-bubble-popping\b/g, "") + " fffg-bubble-popped";
    }, 260);
  }

  function handleBubblePointer(event) {
    if (!document.getElementById("background-wrap")) {
      return;
    }

    var point = event.touches && event.touches.length ? event.touches[0] : event;
    var bubble = findBubbleAt(point.clientX, point.clientY);

    if (!bubble) {
      return;
    }

    popBubble(bubble);
    event.preventDefault();
    event.stopPropagation();
  }

  function initBubblePops() {
    if (!document.querySelector("#background-wrap .bubble")) {
      return;
    }

    addBubblePopStyles();
    document.addEventListener("click", handleBubblePointer, true);
    document.addEventListener("touchstart", handleBubblePointer, true);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initBubblePops);
  } else {
    initBubblePops();
  }
}());
